namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Common.Core.Utils;
using Helper.Core.Domain;
using Helper.Core.Utils;
using Microsoft.Extensions.Logging;

public class PublishingService : IPublishingService
{
    private const string Recommendations = "Recommendations";
    private const string SellPuts = "Sell Puts";
    private const string SellCalls = "Sell Calls";
    private const string OpenInterest = "OI";
    private readonly IRecommendationService recommendationService;
    private readonly IWatchListService watchListService;
    private readonly IMarketDataService marketDataService;
    private readonly IKanbanService kanbanService;
    private readonly ILogger<PublishingService> logger;

    public PublishingService(
        IRecommendationService recommendationService,
        IWatchListService watchListService,
        IMarketDataService marketDataService,
        IKanbanService kanbanService,
        ILogger<PublishingService> logger)
    {
        this.recommendationService = recommendationService;
        this.watchListService = watchListService;
        this.marketDataService = marketDataService;
        this.kanbanService = kanbanService;
        this.logger = logger;
    }

    public async Task PublishSellPutsAsync(SellPutsFilter filter)
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishSellPutsAsync));

        var board = await this.GetSellPutsBoardAsync();

        try
        {
            await this.PublishSellPutsAsync(board, filter);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
        }
        finally
        {
            await this.recommendationService.UpdateSellPutsBoardId(board.Id);
            await this.kanbanService.ResetBoardStateAsync(board.Id);
        }
    }

    private async Task<Board> GetSellPutsBoardAsync()
    {
        var now = DateTime.UtcNow;
        var name = $"{Recommendations} ({SellPuts}) {now.ToShortDateString()} {now.ToShortTimeString()}";
        var description = "Calculation...";

        var boardId = await this.recommendationService.FindSellPutsBoardId();
        if (!string.IsNullOrEmpty(boardId))
        {
            await this.recommendationService.UpdateSellPutsBoardId(string.Empty);
            var board = await this.kanbanService.FindBoardAsync(boardId);
            if (board != null)
            {
                board.Name = name;
                board.Description = description;
                await this.kanbanService.UpdateBoardAsync(board);
                return board;
            }
        }

        return await this.kanbanService.CreateBoardAsync(new Board { Name = name, Description = description });
    }

    public async Task PublishSellCallsAsync(SellCallsFilter filter)
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishSellCallsAsync));

        var board = await this.GetSellCallsBoardAsync();

        try
        {
            await this.PublishSellCallsAsync(board, filter);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
        }
        finally
        {
            await this.recommendationService.UpdateSellCallsBoardId(board.Id);
            await this.kanbanService.ResetBoardStateAsync(board.Id);
        }
    }

    public async Task PublishOpenInterestAsync(OpenInterestFilter filter)
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishOpenInterestAsync));

        var board = await this.GetOpenInterestBoardAsync();

        try
        {
            await this.PublishOpenInterestAsync(board, filter);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
        }
        finally
        {
            await this.recommendationService.UpdateOpenInterestBoardId(board.Id);
            await this.kanbanService.ResetBoardStateAsync(board.Id);
        }
    }

    private async Task<Board> GetOpenInterestBoardAsync()
    {
        var now = DateTime.UtcNow;
        var name = $"{Recommendations} ({OpenInterest}) {now.ToShortDateString()} {now.ToShortTimeString()}";
        var description = "Calculation...";

        var boardId = await this.recommendationService.FindOpenInterestBoardId();
        if (!string.IsNullOrEmpty(boardId))
        {
            await this.recommendationService.UpdateOpenInterestBoardId(string.Empty);
            var board = await this.kanbanService.FindBoardAsync(boardId);
            if (board != null)
            {
                board.Name = name;
                board.Description = description;
                await this.kanbanService.UpdateBoardAsync(board);
                return board;
            }
        }

        return await this.kanbanService.CreateBoardAsync(new Board { Name = name, Description = description });
    }

    private async Task<Board> GetSellCallsBoardAsync()
    {
        var now = DateTime.UtcNow;
        var name = $"{Recommendations} ({SellCalls}) {now.ToShortDateString()} {now.ToShortTimeString()}";
        var description = "Calculation...";

        var boardId = await this.recommendationService.FindSellCallsBoardId();
        if (!string.IsNullOrEmpty(boardId))
        {
            await this.recommendationService.UpdateSellCallsBoardId(string.Empty);
            var board = await this.kanbanService.FindBoardAsync(boardId);
            if (board != null)
            {
                board.Name = name;
                board.Description = description;
                await this.kanbanService.UpdateBoardAsync(board);
                return board;
            }
        }

        return await this.kanbanService.CreateBoardAsync(new Board { Name = name, Description = description });
    }

    private async Task PublishSellPutsAsync(Board board, SellPutsFilter filter)
    {
        var operations = await this.recommendationService.SellPutsAsync(filter,
            total =>
            {
                return new ProgressTracker(total, 1,
                    progress =>
                    {
                        this.kanbanService.SetBoardProgressStateAsync(board.Id, progress).GetAwaiter().GetResult();
                    });
            });

        await this.kanbanService.ResetBoardStateAsync(board.Id);

        board.Description = "Publishing...";
        await this.kanbanService.UpdateBoardAsync(board);

        var tracker = new ProgressTracker(operations.Count(), 1,
            progress =>
            {
                this.kanbanService.SetBoardProgressStateAsync(board.Id, progress).GetAwaiter().GetResult();
            });

        await this.RemoveBoardLanesAsync(board);

        var filterLane = await this.kanbanService.CreateBoardLaneAsync(board.Id,
            new Lane { Name = "FILTER", Description = filter.AsDescription() });

        foreach (var group in operations.GroupBy(op => op.Option.Stock.Id).OrderBy(op => op.Key))
        {
            var stockPrices =
                await this.marketDataService.FindStockPricesAsync(new HashSet<string>(new[] { group.Key }));
            var stockPrice = stockPrices.FirstOrDefault();
            var currentPrice = stockPrice?.Last ?? decimal.Zero;
            var watchListItem = await this.watchListService.FindByTickerAsync(group.Key);
            var buyPrice = watchListItem?.BuyPrice ?? decimal.Zero;
            var sellPrice = watchListItem?.SellPrice ?? decimal.Zero;

            var laneTitle = $"${currentPrice} buy ${buyPrice} sell ${sellPrice}";

            var stocksLane = await this.kanbanService.CreateCardLaneAsync(board.Id, filterLane.Id,
                new Lane { Name = group.Key, Description = laneTitle });

            foreach (var opInfo in group.OrderByDescending(op => op.AnnualRoi).Select(OpInfo))
            {
                try
                {
                    await this.kanbanService.CreateCardAsync(board.Id, stocksLane.Id,
                        new Card { Name = opInfo.Item1, Description = opInfo.Item2 });
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, e.Message);
                }

                tracker.Increase();
            }
        }

        var opMap = operations
            .GroupBy(op => op.Option.Stock.Id)
            .ToDictionary(group => group.Key, group => group.ToList());

        board.Description = opMap.Keys
            .OrderBy(ticker => ticker)
            .Aggregate(string.Empty,
                (curr, ticker) =>
                    curr + (string.IsNullOrEmpty(curr) ? "" : ", ") + $"{ticker} ({opMap[ticker].Count})");

        await this.kanbanService.UpdateBoardAsync(board);
    }

    private async Task PublishSellCallsAsync(Board board, SellCallsFilter filter)
    {
        var operations = await this.recommendationService.SellCallsAsync(filter,
            total =>
            {
                return new ProgressTracker(total, 1,
                    progress =>
                    {
                        this.kanbanService.SetBoardProgressStateAsync(board.Id, progress).GetAwaiter().GetResult();
                    });
            });

        await this.kanbanService.ResetBoardStateAsync(board.Id);

        board.Description = "Publishing...";
        await this.kanbanService.UpdateBoardAsync(board);

        var tracker = new ProgressTracker(operations.Count(), 1,
            progress =>
            {
                this.kanbanService.SetBoardProgressStateAsync(board.Id, progress).GetAwaiter().GetResult();
            });

        await this.RemoveBoardLanesAsync(board);

        var filterLane = await this.kanbanService.CreateBoardLaneAsync(board.Id,
            new Lane { Name = "FILTER", Description = filter.AsDescription() });

        foreach (var group in operations.GroupBy(op => op.Option.Stock.Id).OrderBy(op => op.Key))
        {
            var stockPrices =
                await this.marketDataService.FindStockPricesAsync(new HashSet<string>(new[] { group.Key }));
            var stockPrice = stockPrices.FirstOrDefault();
            var currentPrice = stockPrice?.Last ?? decimal.Zero;
            var watchListItem = await this.watchListService.FindByTickerAsync(group.Key);
            var buyPrice = watchListItem?.BuyPrice ?? decimal.Zero;
            var sellPrice = watchListItem?.SellPrice ?? decimal.Zero;

            var laneTitle = $"${currentPrice} buy ${buyPrice} sell ${sellPrice}";

            var stocksLane = await this.kanbanService.CreateCardLaneAsync(board.Id, filterLane.Id,
                new Lane { Name = group.Key, Description = laneTitle });

            foreach (var opInfo in group.OrderByDescending(op => op.AnnualRoi).Select(OpInfo))
            {
                try
                {
                    await this.kanbanService.CreateCardAsync(board.Id, stocksLane.Id,
                        new Card { Name = opInfo.Item1, Description = opInfo.Item2 });
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, e.Message);
                }

                tracker.Increase();
            }
        }

        var opMap = operations
            .GroupBy(op => op.Option.Stock.Id)
            .ToDictionary(group => group.Key, group => group.ToList());

        board.Description = opMap.Keys
            .OrderBy(ticker => ticker)
            .Aggregate(string.Empty,
                (curr, ticker) =>
                    curr + (string.IsNullOrEmpty(curr) ? "" : ", ") + $"{ticker} ({opMap[ticker].Count})");

        await this.kanbanService.UpdateBoardAsync(board);
    }

    private async Task PublishOpenInterestAsync(Board board, OpenInterestFilter filter)
    {
        var options = await this.recommendationService.OpenInterestAsync(filter,
            total =>
            {
                return new ProgressTracker(total, 1,
                    progress =>
                    {
                        this.kanbanService.SetBoardProgressStateAsync(board.Id, progress).GetAwaiter().GetResult();
                    });
            });

        await this.kanbanService.ResetBoardStateAsync(board.Id);

        board.Description = "Publishing...";
        await this.kanbanService.UpdateBoardAsync(board);

        var tracker = new ProgressTracker(options.Count(), 1,
            progress =>
            {
                this.kanbanService.SetBoardProgressStateAsync(board.Id, progress).GetAwaiter().GetResult();
            });

        await this.RemoveBoardLanesAsync(board);

        var filterLane = await this.kanbanService.CreateBoardLaneAsync(board.Id,
            new Lane { Name = "FILTER", Description = filter.AsDescription() });

        foreach (var group in options.GroupBy(op => OptionUtils.GetStock(op.Ticker)).OrderBy(op => op.Key))
        {
            var stockPrices =
                await this.marketDataService.FindStockPricesAsync(new HashSet<string>(new[] { group.Key }));
            var stockPrice = stockPrices.FirstOrDefault();
            var currentPrice = stockPrice?.Last ?? decimal.Zero;
            var watchListItem = await this.watchListService.FindByTickerAsync(group.Key);
            var buyPrice = watchListItem?.BuyPrice ?? decimal.Zero;
            var sellPrice = watchListItem?.SellPrice ?? decimal.Zero;

            var laneTitle = $"${currentPrice} buy ${buyPrice} sell ${sellPrice}";

            var stocksLane = await this.kanbanService.CreateCardLaneAsync(board.Id, filterLane.Id,
                new Lane { Name = group.Key, Description = laneTitle });

            foreach (var opInfo in group.OrderByDescending(op => op.OpenInterestChangePercent).Select(OpInfo))
            {
                try
                {
                    await this.kanbanService.CreateCardAsync(board.Id, stocksLane.Id,
                        new Card { Name = opInfo.Item1, Description = opInfo.Item2 });
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, e.Message);
                }

                tracker.Increase();
            }
        }

        var opMap = options
            .GroupBy(op => OptionUtils.GetStock(op.Ticker))
            .ToDictionary(group => group.Key, group => group.ToList());

        board.Description = opMap.Keys
            .OrderBy(ticker => ticker)
            .Aggregate(string.Empty,
                (curr, ticker) =>
                    curr + (string.IsNullOrEmpty(curr) ? "" : ", ") + $"{ticker} ({opMap[ticker].Count})");

        await this.kanbanService.UpdateBoardAsync(board);
    }

    private static Tuple<string, string> OpInfo(OpenInterestRecommendation op)
    {
        var exp = Expiration.FromYYYYMMDD(OptionUtils.GetExpiration(op.Ticker));
        var name = $"{OptionUtils.GetSide(op.Ticker)}${OptionUtils.GetStrike(op.Ticker)} {FormatUtils.FormatExpiration(exp.AsDate())}";

        var labelStyle = RenderUtils.CreateStyle(new Tuple<string, string>("whiteSpace", "nowrap"));
        var valueStyle = RenderUtils.CreateStyle(new Tuple<string, string>("paddingLeft", "1rem"));
        var oiValueStyle = RenderUtils.CreateStyle(new Tuple<string, string>("paddingLeft", "1rem"), op.OpenInterestChange < decimal.Zero ? RenderUtils.Red : RenderUtils.Green);

        var list = new List<Tuple<string, string>>();

        list.Add(new Tuple<string, string>("oi", RenderUtils.PropToContent($"{op.OpenInterest}", valueStyle)));
        list.Add(new Tuple<string, string>("oi\u0394#", RenderUtils.PropToContent($"{Math.Abs(Math.Round(op.OpenInterestChange, 0))}", oiValueStyle)));
        list.Add(new Tuple<string, string>("oi\u0394%", RenderUtils.PropToContent(FormatUtils.FormatPercent(Math.Abs(op.OpenInterestChangePercent)), oiValueStyle)));
        list.Add(new Tuple<string, string>("vol", RenderUtils.PropToContent($"{op.Vol}", valueStyle)));
        list.Add(new Tuple<string, string>("bid", RenderUtils.PropToContent(FormatUtils.FormatPrice(op.Bid*100), valueStyle)));
        list.Add(new Tuple<string, string>("ask", RenderUtils.PropToContent(FormatUtils.FormatPrice(op.Ask*100), valueStyle)));
        list.Add(new Tuple<string, string>("last", RenderUtils.PropToContent(FormatUtils.FormatPrice(op.Last*100), valueStyle)));        
        list.Add(new Tuple<string, string>("dte", RenderUtils.PropToContent($"{op.DaysTillExpiration}", valueStyle)));
        

        var body = list
            .Select(x =>
                RenderUtils.PairToContent(RenderUtils.PropToContent(x.Item1, labelStyle), x.Item2)).Aggregate((curr, x) => $"{curr},{x}");

        return new Tuple<string, string>(name, "[" + body + "]");
    }

    private static Tuple<string, string> OpInfo(SellOperation op)
    {
        var name =
            $"{op.Option.Id.OptionType.ToString().Substring(0, 1)}${op.Option.Id.Strike} {(int)op.Option.Id.Expiration.Month}/{op.Option.Id.Expiration.Day}/{op.Option.Id.Expiration.Year}";

        var labelStyle = RenderUtils.CreateStyle(new Tuple<string, string>("whiteSpace", "nowrap"));
        var valueStyle = RenderUtils.CreateStyle(new Tuple<string, string>("paddingLeft", "2rem"));

        var list = new List<Tuple<string, string>>();

        list.Add(new Tuple<string, string>("premium", $"${op.ContractPrice}"));
        list.Add(new Tuple<string, string>("annual roi", $"{CalculationUtils.Percent(op.AnnualRoi, 2)}%"));
        list.Add(new Tuple<string, string>("roi", $"{CalculationUtils.Percent(op.Roi, 2)}%"));
        list.Add(new Tuple<string, string>("collateral", $"${op.Option.Collateral}"));
        list.Add(new Tuple<string, string>("break even", $"${op.BreakEvenStockPrice}"));
        list.Add(new Tuple<string, string>("dte", $"{op.Option.DaysTillExpiration}"));
        list.Add(new Tuple<string, string>("itm", $"{op.Option.InTheMoney}"));

        var body = list
            .Select(x =>
                RenderUtils.PairToContent(RenderUtils.PropToContent(x.Item1, labelStyle),
                    RenderUtils.PropToContent(x.Item2, valueStyle))).Aggregate((curr, x) => $"{curr},{x}");

        return new Tuple<string, string>(name, "[" + body + "]");
    }

    private async Task RemoveBoardLanesAsync(Board board)
    {
        var lanes = await this.kanbanService.FindBoardLanesAsync(board.Id);
        foreach (var laneId in lanes.Select(lane => lane.Id))
        {
            await this.kanbanService.RemoveBoardLaneAsync(board.Id, laneId);
        }
    }
}