namespace Assistant.Tenant.Core.Services;

using System.Security.AccessControl;
using Assistant.Tenant.Core.Models;
using Common.Core.Utils;
using Helper.Core.Domain;
using Helper.Core.Utils;
using Microsoft.Extensions.Logging;

public class PublishingService : IPublishingService
{
    private const string Positions = "Positions";
    private const string Suggestions = "Suggestions";
    private readonly ISuggestionService suggestionService;
    private readonly IPositionService positionService;
    private readonly IMarketDataService marketDataService;
    private readonly IWatchListService watchListService;
    private readonly IKanbanService kanbanService;
    private readonly ILogger<PublishingService> logger;

    public PublishingService(ISuggestionService suggestionService, IPositionService positionService,
        IMarketDataService marketDataService, IWatchListService watchListService,
        IKanbanService kanbanService,
        ILogger<PublishingService> logger)
    {
        this.suggestionService = suggestionService;
        this.positionService = positionService;
        this.marketDataService = marketDataService;
        this.watchListService = watchListService;
        this.kanbanService = kanbanService;
        this.logger = logger;
    }
    
    public async Task PublishPositionsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishPositionsAsync));

        var now = DateTime.UtcNow;
        var board = await this.kanbanService.CreateBoardAsync(new Board
            { Name = $"{Positions} {now.ToShortDateString()} {now.ToShortTimeString()}" });

        try
        {
            await this.PublishPositionsAsync(board);
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

    private async Task PublishPositionsAsync(Board board)
    {
        var positions = (await this.positionService.FindAllAsync()).ToList();
        var tickers = positions.Select(p => p.Type == AssetType.Stock ? p.Ticker : OptionUtils.GetStock(p.Ticker))
            .Distinct().ToHashSet();
        var stocks = (await this.marketDataService.FindStockPricesAsync(tickers)).ToDictionary(stock => stock.Ticker);
        var expirations = new Dictionary<string, IEnumerable<AssetPrice>>();

        var tracker = new ProgressTracker(positions.Count, 1,
            progress =>
            {
                this.kanbanService.SetBoardProgressStateAsync(board.Id, progress);

                this.logger.LogInformation($"{progress}");
            });

        var totalOptions = 0;
        var totalCombos = 0;
        var totalStocks = 0;

        foreach (var entity in positions.GroupBy(p => p.Account, p => p))
        {
            var account = await this.kanbanService.CreateBoardLaneAsync(board.Id,
                new Lane { Name = "ACCOUNT", Description = Hide(entity.Key) });

            var accountPositions = entity.ToList();

            var optionGroups = accountPositions
                .Where(p => p.Type == AssetType.Option)
                .GroupBy(p => p.Tag ?? string.Empty, p => p)
                .ToDictionary(g => g.Key, g => g.ToList());

            totalOptions += await this.PublishOptionsAsync(board, account, optionGroups, stocks, expirations, tracker);
            totalCombos += await this.PublishCombosAsync(board, account, optionGroups, stocks, expirations, tracker);
            totalStocks +=
                await this.PublishStocksAsync(board, account, accountPositions, stocks, expirations, tracker);
        }
        
        tracker.Finish();

        board.Description = $"Stocks ({totalStocks}), Options ({totalOptions}), Combos ({totalCombos})";
        await this.kanbanService.UpdateBoardAsync(board);
    }

    private async Task<int> PublishOptionsAsync(Board board, Lane account,
        IDictionary<string, List<Position>> optionGroups,
        IDictionary<string, AssetPrice> stocks, IDictionary<string, IEnumerable<AssetPrice>> expirations,
        ProgressTracker tracker)
    {
        var singleOptionPositions = optionGroups.Where(g => string.IsNullOrEmpty(g.Key) || g.Value.Count == 1)
            .SelectMany(g => g.Value).ToList();

        if (singleOptionPositions.Count == 0)
        {
            return 0;
        }

        var options = await this.kanbanService.CreateCardLaneAsync(board.Id, account.Id,
            new Lane { Name = "OPTIONS", Description = $"{singleOptionPositions.Count}" });

        foreach (var p in singleOptionPositions.OrderBy(p => p.Ticker))
        {
            var name =
                $"{OptionUtils.GetSide(p.Ticker)}${OptionUtils.GetStrike(p.Ticker)} {FormatExpiration(OptionUtils.ParseExpiration(p.Ticker))}";
            var description = this.PositionToContent(p, stocks, expirations);

            await this.kanbanService.CreateCardAsync(board.Id, options.Id,
                new Card { Name = name, Description = description });

            tracker.Increase();
        }

        return singleOptionPositions.Count;
    }

    private async Task<int> PublishStocksAsync(Board board, Lane account, IEnumerable<Position> positions,
        IDictionary<string, AssetPrice> stocks, IDictionary<string, IEnumerable<AssetPrice>> expirations,
        ProgressTracker tracker)
    {
        var stockPositions = positions.Where(p => p.Type == AssetType.Stock).ToList();

        if (stockPositions.Count == 0)
        {
            return 0;
        }

        var stocksLane = await this.kanbanService.CreateCardLaneAsync(board.Id, account.Id,
            new Lane { Name = "STOCKS", Description = $"{stockPositions.Count}" });

        foreach (var p in stockPositions.OrderBy(p => p.Ticker))
        {
            var name = p.Ticker;
            var description = this.PositionToContent(p, stocks, expirations);

            await this.kanbanService.CreateCardAsync(board.Id, stocksLane.Id,
                new Card { Name = name, Description = description });

            tracker.Increase();
        }

        return stockPositions.Count;
    }

    private async Task<int> PublishCombosAsync(Board board, Lane account,
        IDictionary<string, List<Position>> optionGroups, IDictionary<string, AssetPrice> stocks,
        IDictionary<string, IEnumerable<AssetPrice>> expirations, ProgressTracker tracker)
    {
        var comboOptionPositions = optionGroups.Where(g => !string.IsNullOrEmpty(g.Key) && g.Value.Count > 1).ToList();

        if (comboOptionPositions.Count == 0)
        {
            return 0;
        }

        var combos = await this.kanbanService.CreateCardLaneAsync(board.Id, account.Id,
            new Lane { Name = "COMBOS", Description = $"{comboOptionPositions.Count}" });

        foreach (var p in comboOptionPositions.OrderBy(p => p.Key))
        {
            var leg = p.Value.First();
            var name =
                $"{OptionUtils.GetStock(leg.Ticker)} {FormatExpiration(OptionUtils.ParseExpiration(leg.Ticker))}";

            var description = this.PositionToContent(p.Value, stocks, expirations);

            await this.kanbanService.CreateCardAsync(board.Id, combos.Id,
                new Card { Name = name, Description = description });

            tracker.Increase(p.Value.Count);
        }

        return comboOptionPositions.Count;
    }

    private static int RevealComboSize(IEnumerable<Position> legs)
    {
        var list = legs.ToList();
        var size = 1;

        while (list.All(leg => Math.Abs(leg.Quantity / 2) >= decimal.One))
        {
            list.ForEach(leg => leg.Quantity /= 2);
            size++;
        }

        return size;
    }

    private string PositionToContent(IEnumerable<Position> legs, IDictionary<string, AssetPrice> stocks,
        IDictionary<string, IEnumerable<AssetPrice>> expirations)
    {
        var size = RevealComboSize(legs);

        legs = legs.OrderBy(leg => leg.Type).ThenByDescending(leg => OptionUtils.GetStrike(leg.Ticker));

        return "["
               + legs.Select(leg => LegToContent(leg, stocks, expirations)).Aggregate((curr, el) => $"{curr}, {el}") +
               ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("underlying"),
                   RenderUtils.PropToContent(FormatPrice(stocks[OptionUtils.GetStock(legs.First().Ticker)].Last))) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("quantity"),
                   RenderUtils.PropToContent($"{size}")) + ","
               + this.PriceToContent(legs, expirations) +
               "]";
    }

    private static readonly IDictionary<string, string> FontStyle = new Dictionary<string, string>
    {
        ["fontSize"] = "50%"
    };

    private string LegToContent(Position leg, IDictionary<string, AssetPrice> stocks,
        IDictionary<string, IEnumerable<AssetPrice>> expirations)
    {
        var label = $"{OptionUtils.GetSide(leg.Ticker)}${OptionUtils.GetStrike(leg.Ticker)}";
        var key = RenderUtils.PropToContent(label);
        var value = RenderUtils.PropToContent(FormatSize(leg.Quantity));

        return RenderUtils.PairToContent(key, value) + "," +
               this.PriceToContent(leg, stocks, expirations, false, string.Empty, FontStyle);
    }

    private string PositionToContent(Position pos, IDictionary<string, AssetPrice> stocks,
        IDictionary<string, IEnumerable<AssetPrice>> expirations)
    {
        return pos.Type == AssetType.Stock
            ? this.StockPositionToContent(pos, stocks, expirations)
            : this.OptionPositionToContent(pos, stocks, expirations);
    }

    private string StockPositionToContent(Position pos, IDictionary<string, AssetPrice> stocks,
        IDictionary<string, IEnumerable<AssetPrice>> expirations)
    {
        var ticker = pos.Ticker;

        return "["
               + RenderUtils.PairToContent(RenderUtils.PropToContent("ticker"), RenderUtils.PropToContent(ticker)) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("stock"),
                   RenderUtils.PropToContent(FormatPrice(stocks[ticker].Last))) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("quantity"),
                   RenderUtils.PropToContent(FormatSize(pos.Quantity))) + ","
               + this.PriceToContent(pos, stocks, expirations) +
               "]";
    }

    private string OptionPositionToContent(Position pos, IDictionary<string, AssetPrice> stocks,
        IDictionary<string, IEnumerable<AssetPrice>> expirations)
    {
        if (OptionUtils.GetSide(pos.Ticker) == "P" && pos.Quantity < decimal.Zero)
        {
            return this.ShortPutToContent(pos, stocks, expirations);
        }

        var ticker = OptionUtils.GetStock(pos.Ticker);

        return "["
               + RenderUtils.PairToContent(RenderUtils.PropToContent("ticker"), RenderUtils.PropToContent(ticker)) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("underlying"),
                   RenderUtils.PropToContent(FormatPrice(stocks[ticker].Last))) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("quantity"),
                   RenderUtils.PropToContent(FormatSize(pos.Quantity))) + ","
               + this.PriceToContent(pos, stocks, expirations) +
               "]";
    }

    private string ShortPutToContent(Position pos, IDictionary<string, AssetPrice> stocks,
        IDictionary<string, IEnumerable<AssetPrice>> expirations)
    {
        var ticker = OptionUtils.GetStock(pos.Ticker);
        var strike = OptionUtils.GetStrike(pos.Ticker);

        var watchListItem = stocks[ticker];
        var underlying = watchListItem.Last ?? decimal.Zero;
        var collateral = strike * 100.0m;
        var breakEven = (collateral - pos.AverageCost) / 100.0m;
        var style = breakEven == underlying ? null :
            breakEven < underlying ? RenderUtils.GreenStyle : RenderUtils.RedStyle;
        var breakEvenValue = $"{FormatPrice(breakEven)} ({FormatPrice(Math.Abs(underlying - breakEven))})";
        var dte = Expiration.From(OptionUtils.ParseExpiration(pos.Ticker)).DaysTillExpiration;
        var itm = underlying < strike;

        return "["
               + RenderUtils.PairToContent(RenderUtils.PropToContent("ticker"), RenderUtils.PropToContent(ticker)) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("quantity"),
                   RenderUtils.PropToContent(FormatSize(pos.Quantity))) + ","
               + this.PriceToContent(pos, stocks, expirations) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("underlying"),
                   RenderUtils.PropToContent(FormatPrice(underlying))) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("collateral"),
                   RenderUtils.PropToContent(FormatPrice(collateral))) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("break.even"),
                   RenderUtils.PropToContent(breakEvenValue, style)) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("dte"), RenderUtils.PropToContent($"{dte}")) + ","
               + RenderUtils.PairToContent(RenderUtils.PropToContent("itm"),
                   RenderUtils.PropToContent($"{itm}", itm ? RenderUtils.RedStyle : RenderUtils.GreenStyle)) +
               "]";
    }

    private string PriceToContent(IEnumerable<Position> legs, IDictionary<string, IEnumerable<AssetPrice>> expirations)
    {
        const string label = "price";
        var comboChange = new Tuple<decimal, decimal>(decimal.Zero, decimal.Zero);

        foreach (var leg in legs)
        {
            var legChange = this.GetOptionPriceChange(leg, expirations);

            if (legChange == null)
            {
                legChange = new Tuple<decimal, decimal, decimal>(leg.AverageCost, decimal.Zero, decimal.Zero);
            }

            if (leg.Quantity > decimal.Zero)
            {
                comboChange = new Tuple<decimal, decimal>(comboChange.Item1 + legChange.Item1,
                    comboChange.Item2 + legChange.Item2);
            }
            else
            {
                comboChange = new Tuple<decimal, decimal>(comboChange.Item1 - legChange.Item1,
                    comboChange.Item2 - legChange.Item2);
            }
        }

        if (comboChange.Item2 == decimal.Zero)
        {
            return RenderUtils.PairToContent(
                RenderUtils.PropToContent(label),
                RenderUtils.PropToContent($"${Math.Round(comboChange.Item1, 2)}"));
        }

        var style = comboChange.Item2 > decimal.Zero ? RenderUtils.GreenStyle : RenderUtils.RedStyle;

        return RenderUtils.PairToContent(
            RenderUtils.PropToContent(label),
            RenderUtils.PropToContent(
                $"${Math.Round(comboChange.Item1, 2)} (${Math.Round(Math.Abs(comboChange.Item2), 2)})", style));
    }

    private string PriceToContent(Position pos, IDictionary<string, AssetPrice> stocks,
        IDictionary<string, IEnumerable<AssetPrice>> expirations, bool percents = true, string? label = null,
        IDictionary<string, string> extraValues = null)
    {
        label = label ?? "price";
        if (pos.Type == AssetType.Stock)
        {
            var priceChange = GetStockPriceChange(pos, stocks);
            if (priceChange != null)
            {
                var content = percents
                    ? $"${priceChange.Item1} ({Math.Abs(priceChange.Item3)}%)"
                    : $"${priceChange.Item1} (${Math.Abs(priceChange.Item2)})";

                return RenderUtils.PairToContent(
                    RenderUtils.PropToContent(label),
                    RenderUtils.PropToContent(content, GetStyle(pos, priceChange.Item2, extraValues)));
            }
        }
        else
        {
            var priceChange = this.GetOptionPriceChange(pos, expirations);
            if (priceChange != null)
            {
                var content = percents
                    ? $"${priceChange.Item1} ({Math.Abs(priceChange.Item3)}%)"
                    : $"${priceChange.Item1} (${Math.Abs(priceChange.Item2)})";

                return RenderUtils.PairToContent(
                    RenderUtils.PropToContent(label),
                    RenderUtils.PropToContent(content, GetStyle(pos, priceChange.Item2, extraValues)));
            }
        }

        return RenderUtils.PairToContent(
            RenderUtils.PropToContent(label),
            RenderUtils.PropToContent($"${Math.Round(pos.AverageCost, 2)}"));
    }

    private static Tuple<decimal, decimal, decimal> GetStockPriceChange(Position pos, IDictionary<string, AssetPrice> stocks)
    {
        var ticker = pos.Ticker;

        var price = stocks.ContainsKey(ticker) ? stocks[ticker] : null;

        if (price == null)
        {
            return null;
        }

        var diff = (price.Last ?? decimal.Zero) - pos.AverageCost;

        return new Tuple<decimal, decimal, decimal>(Math.Round(pos.AverageCost, 2), Math.Round(diff, 2),
            CalculationUtils.Percent(diff / pos.AverageCost, 2));
    }

    private Tuple<decimal, decimal, decimal> GetOptionPriceChange(Position pos,
        IDictionary<string, IEnumerable<AssetPrice>> context)
    {
        var ticker = OptionUtils.GetStock(pos.Ticker);
        var expiration = OptionUtils.GetExpiration(pos.Ticker);
        var strike = OptionUtils.GetStrike(pos.Ticker);
        var side = OptionUtils.GetSide(pos.Ticker);

        var key = $"{ticker}-{expiration}";

        var prices = context != null && context.ContainsKey(key)
            ? context[key]
            : this.marketDataService.FindOptionPricesAsync(ticker, expiration).Result;

        if (prices == null)
        {
            return null;
        }

        if (context != null && !context.ContainsKey(key))
        {
            context.Add(key, prices);
        }

        var priceHistory = prices.FirstOrDefault(p =>
            OptionUtils.GetSide(p.Ticker) == side && OptionUtils.GetStrike(p.Ticker) == strike);
        if (priceHistory == null)
        {
            return null;
        }

        var price = (priceHistory.Last ?? decimal.Zero) * 100;
        var diff = price - pos.AverageCost;

        return new Tuple<decimal, decimal, decimal>(Math.Round(pos.AverageCost, 2), Math.Round(diff, 2),
            CalculationUtils.Percent(diff / pos.AverageCost, 2));
    }

    private static IDictionary<string, string> GetStyle(Position pos, decimal priceChange,
        IDictionary<string, string> extraValues = null)
    {
        var isLong = pos.Quantity > decimal.Zero;
        var isShort = !isLong;

        var isPriceIncreased = priceChange > decimal.Zero;
        var isPriceDecreased = !isPriceIncreased;

        var isProfit = (isLong && isPriceIncreased) || (isShort && isPriceDecreased);

        var style = isProfit ? RenderUtils.GreenStyle : RenderUtils.RedStyle;

        if (extraValues != null)
        {
            foreach (var pair in extraValues)
            {
                style.Add(pair.Key, pair.Value);
            }
        }

        return style;
    }

    private static string FormatPrice(decimal? price)
    {
        return $"${Math.Round(price ?? decimal.Zero, 2)}";
    }

    private static string FormatSize(decimal size)
    {
        return size < 0 ? $"short({Math.Abs(size)})" : $"long({size})";
    }

    private static string FormatExpiration(DateTime expiraion)
    {
        return $"{expiraion.Year}/{expiraion.Month}/{expiraion.Day}";
    }

    private static string Hide(string value)
    {
        return value.Substring(0, 2) + new string(Enumerable.Repeat('*', value.Length - 2).ToArray());
    }

    public async Task PublishSuggestionsAsync(SuggestionFilter filter)
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishSuggestionsAsync));

        var now = DateTime.UtcNow;
        
        var board = await this.kanbanService.CreateBoardAsync(new Board
        {
            Name = $"{Suggestions} {now.ToShortDateString()} {now.ToShortTimeString()}", 
            Description = "Calculation..."
        });

        try
        {
            await this.PublishSuggestionsAsync(board, filter);
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

    private async Task PublishSuggestionsAsync(Board board, SuggestionFilter filter)
    {
        var operations = await this.suggestionService.SuggestPutsAsync(filter,
            total =>
            {
                return new ProgressTracker(total, 1,
                    progress => { this.kanbanService.SetBoardProgressStateAsync(board.Id, progress); });
            });

        await this.kanbanService.ResetBoardStateAsync(board.Id);

        board.Description = "Publishing...";
        await this.kanbanService.UpdateBoardAsync(board);

        var tracker = new ProgressTracker(operations.Count(), 1,
            progress =>
            {
                this.kanbanService.SetBoardProgressStateAsync(board.Id, progress);

                this.logger.LogInformation($"{progress}");
            });

        var putsLane = await this.kanbanService.CreateBoardLaneAsync(board.Id,
            new Lane { Name = "PUTS", Description = filter.AsDescription() });

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
                await this.kanbanService.CreateCardAsync(board.Id, stocksLane.Id,
                    new Card { Name = opInfo.Item1, Description = opInfo.Item2 });

                tracker.Increase();
            }
        }
        
        board.Description = operations.Select(item => item.Option.Stock.Id).Distinct().OrderBy(ticker => ticker)
            .Aggregate("", (curr, el) => curr + (string.IsNullOrEmpty(curr) ? "" : ", ") + el);

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