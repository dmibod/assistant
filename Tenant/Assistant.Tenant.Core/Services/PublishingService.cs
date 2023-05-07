namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Common.Core.Utils;
using Helper.Core.Domain;
using Helper.Core.Utils;
using Microsoft.Extensions.Logging;

public class PublishingService : IPublishingService
{
    private const string Recommendations = "Recommendations";
    private const string SellPuts = "sell puts";
    private const string SellCalls = "sell calls";
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

    public async Task PublishSellPutsAsync(RecommendationFilter filter)
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishSellPutsAsync));

        var board = await this.GetBoardAsync(SellPuts);

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
            await this.kanbanService.ResetBoardStateAsync(board.Id);
        }
    }

    public async Task PublishSellCallsAsync(RecommendationFilter filter, bool considerPositions)
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishSellCallsAsync));

        var board = await this.GetBoardAsync(SellCalls);

        try
        {
            await this.PublishSellCallsAsync(board, filter, considerPositions);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, e.Message);
        }
        finally
        {
            await this.kanbanService.ResetBoardStateAsync(board.Id);
        }
    }

    private async Task<Board> GetBoardAsync(string prefix)
    {
        var boards = await this.kanbanService.FindBoardsAsync();
        
        var board = boards.FirstOrDefault(board => board.Name.StartsWith($"{Recommendations} ({prefix})"));

        if (board != null)
        {
            await this.kanbanService.RemoveBoardAsync(board.Id);
        }
        
        var now = DateTime.UtcNow;
        return await this.kanbanService.CreateBoardAsync(new Board
        {
            Name = $"{Recommendations} ({prefix}) {now.ToShortDateString()} {now.ToShortTimeString()}",
            Description = "Calculation..."
        });
    }

    private async Task PublishSellPutsAsync(Board board, RecommendationFilter filter)
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

        var putsLane = await this.kanbanService.CreateBoardLaneAsync(board.Id,
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

            var stocksLane = await this.kanbanService.CreateCardLaneAsync(board.Id, putsLane.Id,
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

    private async Task PublishSellCallsAsync(Board board, RecommendationFilter filter, bool considerPositions)
    {
        var operations = await this.recommendationService.SellCallsAsync(filter, considerPositions,
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

        var putsLane = await this.kanbanService.CreateBoardLaneAsync(board.Id,
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

            var stocksLane = await this.kanbanService.CreateCardLaneAsync(board.Id, putsLane.Id,
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
}