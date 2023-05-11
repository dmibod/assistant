namespace Assistant.Market.Core.Services;

using System.Text.RegularExpressions;
using Assistant.Market.Core.Models;
using Common.Core.Utils;
using Microsoft.Extensions.Logging;

public class PublishingService : IPublishingService
{
    private static readonly IDictionary<string, string> FontStyle = new Dictionary<string, string>
    {
        ["fontWeight"] = "500"
    };

    private const string MarketData = "Market Data";
    private const string OpenInterest = "Open Interest";
    private const int ChunkSize = 10;

    private readonly IStockService stockService;
    private readonly IOptionService optionService;
    private readonly IKanbanService kanbanService;
    private readonly ILogger<PublishingService> logger;

    public PublishingService(IStockService stockService, IOptionService optionService, IKanbanService kanbanService,
        ILogger<PublishingService> logger)
    {
        this.stockService = stockService;
        this.optionService = optionService;
        this.kanbanService = kanbanService;
        this.logger = logger;
    }

    public async Task PublishAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishAsync));

        var stocks = await this.stockService.FindAllAsync();
        var map = stocks.ToDictionary(stock => stock.Ticker);
        var counter = 1;

        foreach (var chunk in map.Values.OrderBy(stock => stock.Ticker).Chunk(ChunkSize))
        {
            await this.PublishAsync(counter++, chunk);
        }
    }

    public async Task PublishOpenInterestAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishOpenInterestAsync));

        var boards = await this.kanbanService.FindBoardsAsync();
        var board = boards.FirstOrDefault(board => board.Name.StartsWith(OpenInterest));

        var now = DateTime.UtcNow;
        var name = $"{OpenInterest} (Today) {now.ToShortDateString()} {now.ToShortTimeString()}";

        if (board != null)
        {
            board.Name = name;
            board.Description = "Calculation...";
            await this.kanbanService.UpdateBoardAsync(board);
        }
        else
        {
            board = await this.kanbanService.CreateBoardAsync(new Board
                { Name = name, Description = "Calculation..." });
        }

        await this.PublishOpenInterestAsync(board);
    }

    private async Task PublishOpenInterestAsync(Board board)
    {
        try
        {
            await this.kanbanService.SetBoardLoadingStateAsync(board.Id);

            await this.RemoveBoardLanesAsync(board);

            var tickers = await this.stockService.FindTickersAsync();

            var dictionary = new Dictionary<string, int>();

            foreach (var ticker in tickers.OrderBy(t => t))
            {
                var count = await this.optionService.FindChangesCountAsync(ticker);

                if (count > 0)
                {
                    dictionary.Add(ticker, count);
                }
            }

            var lane = await this.kanbanService.CreateCardLaneAsync(board.Id, board.Id, new Lane
            {
                Name = OpenInterest,
                Description = "companies ordered by the number of OI changes (max > min)"
            });

            foreach (var pair in dictionary.OrderByDescending(p => p.Value))
            {
                var min = await this.optionService.FindOpenInterestChangeMinAsync(pair.Key);
                var max = await this.optionService.FindOpenInterestChangeMaxAsync(pair.Key);
                var percMin = await this.optionService.FindOpenInterestChangePercentMinAsync(pair.Key);
                var percMax = await this.optionService.FindOpenInterestChangePercentMaxAsync(pair.Key);

                var propMin = RenderUtils.PairToContent(
                    RenderUtils.PropToContent( /*"oi\u0394\u2193#"*/"min \u0394#"),
                    RenderUtils.PropToContent(FormatUtils.FormatAbsNumber(min), GetNumberStyle(min)));

                var propPercMin = RenderUtils.PairToContent(
                    RenderUtils.PropToContent( /*"oi\u0394\u2193%"*/"min \u0394%"),
                    RenderUtils.PropToContent(FormatUtils.FormatAbsPercent(percMin, 2), GetNumberStyle(percMin)));

                var propMax = RenderUtils.PairToContent(
                    RenderUtils.PropToContent( /*"oi\u0394\u2191#"*/"max \u0394#"),
                    RenderUtils.PropToContent(FormatUtils.FormatAbsNumber(max), GetNumberStyle(max)));

                var propPercMax = RenderUtils.PairToContent(
                    RenderUtils.PropToContent( /*"oi\u0394\u2191%"*/"max \u0394%"),
                    RenderUtils.PropToContent(FormatUtils.FormatAbsPercent(percMax, 0), GetNumberStyle(percMax)));

                await this.kanbanService.CreateCardAsync(board.Id, lane.Id, new Card
                {
                    Name = $"{pair.Key} ({pair.Value})",
                    Description = $"[{propMin}, {propPercMin}, {propMax}, {propPercMax}]"
                });
            }

            if (dictionary.Count > 0)
            {
                board.Description = dictionary.Keys.Count > 100
                    ? dictionary.Keys.Take(100).Aggregate((curr, i) => $"{curr}, {i}") + "..."
                    : dictionary.Keys.Aggregate((curr, i) => $"{curr}, {i}");
            }
            else
            {
                board.Description = string.Empty;
            }

            await this.kanbanService.UpdateBoardAsync(board);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to publish open interest data for {Board} with {Content}", board.Name,
                board.Description);
        }
        finally
        {
            await this.kanbanService.ResetBoardStateAsync(board.Id);
        }
    }

    private static IEnumerable<Tuple<string, string>> YieldNumberStyle(decimal number)
    {
        switch (number)
        {
            case < decimal.Zero:
                yield return RenderUtils.Red;
                break;
            case > decimal.Zero:
                yield return RenderUtils.Green;
                break;
        }

        yield return new Tuple<string, string>("paddingLeft", "1rem");
    }

    private static IDictionary<string, string> GetNumberStyle(decimal number)
    {
        return RenderUtils.CreateStyle(YieldNumberStyle(number).ToArray());
    }

    private async Task RemoveBoardLanesAsync(Board board)
    {
        var lanes = await this.kanbanService.FindLanesAsync(board.Id);

        foreach (var laneId in lanes.Select(lane => lane.Id))
        {
            await this.kanbanService.RemoveLaneAsync(board.Id, laneId);
        }
    }

    private async Task PublishAsync(int chunkNo, Stock[] chunk)
    {
        var key = $"{MarketData} {chunkNo}";
        var name = $"{key} ({chunk.Length})";
        var description = chunk.Select(item => item.Ticker).Aggregate((curr, i) => $"{curr}, {i}");

        const string pattern = @"\(\d+\)";
        var boards = await this.kanbanService.FindBoardsAsync();
        var board = boards
            .Where(board => board.Name.StartsWith(MarketData))
            .FirstOrDefault(board => Regex.IsMatch(board.Name, $"{key} {pattern}"));

        if (board != null)
        {
            board.Name = name;
            board.Description = description;

            await this.kanbanService.UpdateBoardAsync(board);
        }
        else
        {
            board = await this.kanbanService.CreateBoardAsync(new Board { Name = name, Description = description });
        }

        await this.PublishAsync(board, chunk);
    }

    private async Task PublishAsync(Board board, Stock[] chunk)
    {
        try
        {
            await this.kanbanService.SetBoardLoadingStateAsync(board.Id);

            var stockLanes = await this.PublishTickersAsync(board, chunk);

            await this.PublishExpirationsAsync(board, chunk, stockLanes);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to publish marked data for {Board} with {Content}", board.Name,
                board.Description);
        }
        finally
        {
            await this.kanbanService.ResetBoardStateAsync(board.Id);
        }
    }

    private async Task<IDictionary<string, Lane>> PublishTickersAsync(Board board, Stock[] chunk)
    {
        var lanes = await this.kanbanService
            .FindBoardLanesAsync(board.Id);

        var stockLanes = lanes.ToDictionary(lane => lane.Name, lane => lane);

        foreach (var stock in chunk)
        {
            var description = stock.LastRefresh != DateTime.UnixEpoch
                ? $"${Math.Round(stock.Last, 2)}, {stock.LastRefresh.ToShortDateString()} {stock.LastRefresh.ToShortTimeString()}"
                : "n/a";

            if (!stockLanes.ContainsKey(stock.Ticker))
            {
                var lane = await this.kanbanService
                    .CreateBoardLaneAsync(board.Id, new Lane { Name = stock.Ticker, Description = description });

                stockLanes.Add(stock.Ticker, lane);
            }
            else
            {
                stockLanes[stock.Ticker].Description = description;

                await this.kanbanService
                    .UpdateBoardLaneAsync(board.Id, stockLanes[stock.Ticker]);
            }
        }

        var tickers = chunk.Select(i => i.Ticker).ToHashSet();

        // remove ticker lanes, which are not included in chunk
        foreach (var pair in stockLanes.Where(pair => !tickers.Contains(pair.Key)))
        {
            await this.kanbanService.RemoveLaneAsync(board.Id, pair.Value.Id);
        }

        return stockLanes;
    }

    private async Task PublishExpirationsAsync(Board board, Stock[] chunk, IDictionary<string, Lane> stockLanes)
    {
        foreach (var stock in chunk.Where(i => stockLanes.ContainsKey(i.Ticker)))
        {
            var optionChain = await this.optionService.FindAsync(stock.Ticker);

            var changeChain = await this.optionService.FindChangeAsync(stock.Ticker);

            var stockLane = stockLanes[stock.Ticker];

            var lanes = await this.kanbanService.FindLanesAsync(board.Id, stockLane.Id);

            var expirationLanes = lanes.ToDictionary(l => l.Name);

            foreach (var expiration in optionChain.Expirations.Keys.OrderBy(i => i))
            {
                await this.PublishExpirationAsync(board, stockLane, optionChain, changeChain, expiration,
                    expirationLanes);
            }

            var optionChainExpirations = optionChain.Expirations.Keys.ToHashSet();

            // remove expiration lanes, which are not included in chain
            foreach (var pair in expirationLanes.Where(pair => !optionChainExpirations.Contains(pair.Key)))
            {
                await this.kanbanService.RemoveLaneAsync(board.Id, pair.Value.Id);
            }
        }
    }

    private const string CallsCardName = "CALLS";
    private const string PutsCardName = "PUTS";

    private async Task PublishExpirationAsync(Board board, Lane stockLane, OptionChain chain, OptionChain change,
        string expiration,
        IDictionary<string, Lane> expirationLanes)
    {
        if (!expirationLanes.ContainsKey(expiration))
        {
            var lane = await this.kanbanService.CreateCardLaneAsync(board.Id, stockLane.Id, new Lane
            {
                Name = expiration
            });

            expirationLanes.Add(expiration, lane);
        }

        var expirationLane = expirationLanes[expiration];

        var cards = await this.kanbanService.FindCardsAsync(board.Id, expirationLane.Id);

        var cardsMap = cards.ToDictionary(c => c.Name);

        var changeExpiration = GetChangeExpiration(change, expiration);

        var callDesc = CallsContent(chain.Expirations[expiration], changeExpiration);

        if (!cardsMap.ContainsKey(CallsCardName))
        {
            await this.kanbanService
                .CreateCardAsync(board.Id, expirationLane.Id,
                    new Card { Name = CallsCardName, Description = callDesc });
        }
        else
        {
            cardsMap[CallsCardName].Description = callDesc;

            await this.kanbanService.UpdateCardAsync(board.Id, cardsMap[CallsCardName]);
        }

        var putDesc = PutsContent(chain.Expirations[expiration], changeExpiration);

        if (!cardsMap.ContainsKey(PutsCardName))
        {
            await this.kanbanService
                .CreateCardAsync(board.Id, expirationLane.Id, new Card { Name = PutsCardName, Description = putDesc });
        }
        else
        {
            cardsMap[PutsCardName].Description = putDesc;

            await this.kanbanService.UpdateCardAsync(board.Id, cardsMap[PutsCardName]);
        }
    }

    private static OptionExpiration? GetChangeExpiration(OptionChain change, string expiration)
    {
        if (!change.Expirations.TryGetValue(expiration, out var data))
        {
            return null;
        }

        return data.LastRefresh >= DateTimeUtils.TodayUtc() ? data : null;
    }

    private static OptionContract? GetCallContract(decimal strike, OptionExpiration? change)
    {
        if (change == null || !change.Contracts.TryGetValue(strike, out var contracts))
        {
            return null;
        }

        return contracts.Call;
    }

    private static string CallsContent(OptionExpiration expiration, OptionExpiration? change)
    {
        var strikesWithContracts = expiration.Contracts
            .Where(pair => pair.Value.Call != null)
            .OrderBy(pair => pair.Key)
            .ToList();
        
        var priceTuples = strikesWithContracts.Select(pair => new Tuple<string, Tuple<string, IDictionary<string, string>?>>(DecimalToContent(pair.Key), PriceToContent(pair.Value.Call))).ToList();
        if (priceTuples.Count > 0)
        {
            priceTuples.Insert(0, new Tuple<string, Tuple<string, IDictionary<string, string>>>("Strike", new Tuple<string, IDictionary<string, string>>("bid/ask", FontStyle))); 
        }

        if (change != null)
        {
            var openInterestContracts = strikesWithContracts
                .Select(pair => new KeyValuePair<decimal, OptionContract>(pair.Key, GetCallContract(pair.Key, change)))
                .Where(pair => pair.Value != null && pair.Value.OI != decimal.Zero)
                .OrderBy(pair => pair.Key)
                .Select(pair => new Tuple<string, Tuple<string, IDictionary<string, string>?>>(DecimalToContent(pair.Key), OpenInterestToContent(pair.Value)))
                .ToList();

            if (openInterestContracts.Count > 0)
            {
                openInterestContracts.Insert(0, new Tuple<string, Tuple<string, IDictionary<string, string>>>("Strike", new Tuple<string, IDictionary<string, string>>("oi", FontStyle)));
                priceTuples = priceTuples.Union(openInterestContracts).ToList();
            }
        }

        return TupleToContent(priceTuples);
    }

    private static OptionContract? GetPutContract(decimal strike, OptionExpiration? change)
    {
        if (change == null || !change.Contracts.TryGetValue(strike, out var contracts))
        {
            return null;
        }

        return contracts.Put;
    }

    private static string PutsContent(OptionExpiration expiration, OptionExpiration? change)
    {
        var strikesWithContracts = expiration.Contracts
            .Where(pair => pair.Value.Put != null)
            .OrderBy(pair => pair.Key)
            .ToList();
        
        var priceTuples = strikesWithContracts.Select(pair => new Tuple<string, Tuple<string, IDictionary<string, string>?>>(DecimalToContent(pair.Key), PriceToContent(pair.Value.Put))).ToList();
        if (priceTuples.Count > 0)
        {
            priceTuples.Insert(0, new Tuple<string, Tuple<string, IDictionary<string, string>>>("Strike", new Tuple<string, IDictionary<string, string>>("bid/ask", FontStyle))); 
        }

        if (change != null)
        {
            var openInterestContracts = strikesWithContracts
                .Select(pair => new KeyValuePair<decimal, OptionContract>(pair.Key, GetPutContract(pair.Key, change)))
                .Where(pair => pair.Value != null && pair.Value.OI != decimal.Zero)
                .OrderBy(pair => pair.Key)
                .Select(pair => new Tuple<string, Tuple<string, IDictionary<string, string>?>>(DecimalToContent(pair.Key), OpenInterestToContent(pair.Value)))
                .ToList();

            if (openInterestContracts.Count > 0)
            {
                openInterestContracts.Insert(0, new Tuple<string, Tuple<string, IDictionary<string, string>>>("Strike", new Tuple<string, IDictionary<string, string>>("oi", FontStyle)));
                priceTuples = priceTuples.Union(openInterestContracts).ToList();
            }
        }

        return TupleToContent(priceTuples);
    }

    private static string DecimalToContent(decimal? value)
    {
        return FormatUtils.FormatNumber(value, 2);
    }

    private static Tuple<string, IDictionary<string, string>?> PriceToContent(OptionContract price)
    {
        var content = price.Bid == price.Ask
            ? price.Bid == decimal.Zero ? $"${DecimalToContent(price.Last)}" : $"${DecimalToContent(price.Bid)}"
            : $"${DecimalToContent(price.Bid)} ({DecimalToContent(price.Ask)})";

        return new Tuple<string, IDictionary<string, string>?>(content, null);
    }

    private static Tuple<string, IDictionary<string, string>?> OpenInterestToContent(OptionContract contract)
    {
        var content = $" {FormatUtils.FormatAbsPercent(contract.Vol)} ({FormatUtils.FormatAbsNumber(contract.OI)})";

        return new Tuple<string, IDictionary<string, string>?>(content,
            contract.OI < decimal.Zero ? RenderUtils.RedStyle : RenderUtils.GreenStyle);
    }

    private static string TupleToContent(IEnumerable<Tuple<string, Tuple<string, IDictionary<string, string>?>>> tuples)
    {
        var list = tuples.ToList();
        if (list.Count == 0)
        {
            return string.Empty;
        }

        var body = list
            .Select(x =>
                RenderUtils.PairToContent(RenderUtils.PropToContent(x.Item1),
                    RenderUtils.PropToContent(x.Item2.Item1, x.Item2.Item2)))
            .Aggregate((curr, x) => $"{curr},{x}");

        return $"[{body}]";
    }
}