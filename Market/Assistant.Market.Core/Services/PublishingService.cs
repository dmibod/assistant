namespace Assistant.Market.Core.Services;

using System.Globalization;
using System.Text.RegularExpressions;
using Assistant.Market.Core.Models;
using Common.Core.Utils;
using Microsoft.Extensions.Logging;
using Exception = System.Exception;

public class PublishingService : IPublishingService
{
    private const string MarketData = "Market Data";
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
            this.Publish(counter++, chunk);
        }
    }

    private void Publish(int chunkNo, Stock[] chunk)
    {
        var key = $"{MarketData} {chunkNo}";
        var name = $"{key} ({chunk.Length})";
        var description = chunk.Select(item => item.Ticker).Aggregate((curr, i) => $"{curr}, {i}");

        const string pattern = @"\(\d+\)";
        var board = this.kanbanService.FindBoardsAsync().Result
            .Where(board => board.Name.StartsWith(MarketData))
            .FirstOrDefault(board => Regex.IsMatch(board.Name, $"{key} {pattern}"));

        if (board != null)
        {
            board.Name = name;
            board.Description = description;

            this.kanbanService.UpdateBoardAsync(board).GetAwaiter().GetResult();
        }
        else
        {
            board = this.kanbanService.CreateBoardAsync(new Board
            {
                Name = name,
                Description = description
            }).Result;
        }

        this.Publish(board, chunk);
    }

    private void Publish(Board board, Stock[] chunk)
    {
        try
        {
            this.kanbanService.SetBoardLoadingStateAsync(board.Id).GetAwaiter().GetResult();

            var stockLanes = this.PublishTickers(board, chunk);

            this.PublishExpirations(board, chunk, stockLanes);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to publish marked data for {Board} with {Content}", board.Name,
                board.Description);
        }
        finally
        {
            this.kanbanService.ResetBoardStateAsync(board.Id).GetAwaiter().GetResult();
        }
    }

    private IDictionary<string, Lane> PublishTickers(Board board, Stock[] chunk)
    {
        var stockLanes = this.kanbanService
            .FindBoardLanesAsync(board.Id)
            .Result
            .ToDictionary(lane => lane.Name, lane => lane);

        foreach (var stock in chunk)
        {
            var description = stock.LastRefresh != DateTime.UnixEpoch
                ? $"${Math.Round(stock.Last, 2)}, {stock.LastRefresh.ToShortDateString()} {stock.LastRefresh.ToShortTimeString()}"
                : "n/a";

            if (!stockLanes.ContainsKey(stock.Ticker))
            {
                var lane = this.kanbanService
                    .CreateBoardLaneAsync(board.Id, new Lane { Name = stock.Ticker, Description = description })
                    .Result;

                stockLanes.Add(stock.Ticker, lane);
            }
            else
            {
                stockLanes[stock.Ticker].Description = description;

                this.kanbanService
                    .UpdateBoardLaneAsync(board.Id, stockLanes[stock.Ticker])
                    .GetAwaiter()
                    .GetResult();
            }
        }

        var tickers = chunk.Select(i => i.Ticker).ToHashSet();
        
        // remove ticker lanes, which are not included in chunk
        foreach (var pair in stockLanes.Where(pair => !tickers.Contains(pair.Key)))
        {
            this.kanbanService.RemoveBoardLaneAsync(board.Id, pair.Value.Id).GetAwaiter().GetResult();
        }

        return stockLanes;
    }

    private void PublishExpirations(Board board, Stock[] chunk, IDictionary<string, Lane> stockLanes)
    {
        foreach (var stock in chunk.Where(i => stockLanes.ContainsKey(i.Ticker)))
        {
            var optionChain = this.optionService.FindAsync(stock.Ticker).Result;

            var stockLane = stockLanes[stock.Ticker];
            
            var expirationLanes = this.kanbanService.FindLanesAsync(board.Id, stockLane.Id)
                .Result
                .ToDictionary(l => l.Name);

            foreach (var expiration in optionChain.Expirations.Keys.OrderBy(i => i))
            {
                this.PublishExpiration(board, stockLane, stock, optionChain, expiration, expirationLanes);
            }

            var optionChainExpirations = optionChain.Expirations.Keys.ToHashSet();
            
            // remove expiration lanes, which are not included in chain
            foreach (var pair in expirationLanes.Where(pair => !optionChainExpirations.Contains(pair.Key)))
            {
                this.kanbanService.RemoveBoardLaneAsync(board.Id, pair.Value.Id).GetAwaiter().GetResult();
            }
        }
    }

    private void PublishExpiration(Board board, Lane stockLane, Stock stock, OptionChain chain, string expiration,
        IDictionary<string, Lane> expirationLanes)
    {
        if (!expirationLanes.ContainsKey(expiration))
        {
            var lane = this.kanbanService.CreateCardLaneAsync(board.Id, stockLane.Id, new Lane
            {
                Name = expiration
            }).Result;

            expirationLanes.Add(expiration, lane);
        }

        var expirationLane = expirationLanes[expiration];

        var cardsMap = this.kanbanService.FindCardsAsync(board.Id, expirationLane.Id).Result.ToDictionary(c => c.Name);

        var callDesc = CallsContent(chain.Expirations[expiration]);

        const string CallsCardName = "CALLS";

        if (!cardsMap.ContainsKey(CallsCardName))
        {
            this.kanbanService
                .CreateCardAsync(board.Id, expirationLane.Id, new Card { Name = CallsCardName, Description = callDesc })
                .GetAwaiter().GetResult();
        }
        else
        {
            cardsMap[CallsCardName].Description = callDesc;

            this.kanbanService
                .UpdateCardAsync(board.Id, cardsMap[CallsCardName])
                .GetAwaiter()
                .GetResult();
        }

        var putDesc = PutsContent(chain.Expirations[expiration]);

        const string PutsCardName = "PUTS";

        if (!cardsMap.ContainsKey(PutsCardName))
        {
            this.kanbanService
                .CreateCardAsync(board.Id, expirationLane.Id, new Card { Name = PutsCardName, Description = putDesc })
                .GetAwaiter().GetResult();
        }
        else
        {
            cardsMap[PutsCardName].Description = putDesc;

            this.kanbanService
                .UpdateCardAsync(board.Id, cardsMap[PutsCardName])
                .GetAwaiter()
                .GetResult();
        }
    }

    private static string CallsContent(OptionExpiration expiration)
    {
        var tuples = expiration.Contracts
            .Where(pair => pair.Value.Call != null)
            .OrderBy(pair => pair.Key)
            .Select(pair => new Tuple<string, string>(DecimalToContent(pair.Key), PriceToContent(pair.Value.Call)));

        return TupleToContent(tuples);
    }

    private static string PutsContent(OptionExpiration expiration)
    {
        var tuples = expiration.Contracts
            .Where(pair => pair.Value.Put != null)
            .OrderBy(pair => pair.Key)
            .Select(pair => new Tuple<string, string>(DecimalToContent(pair.Key), PriceToContent(pair.Value.Put)));

        return TupleToContent(tuples);
    }

    private static string DecimalToContent(decimal? value)
    {
        var round = Math.Round(value ?? Decimal.Zero, 2);

        return round.ToString(CultureInfo.InvariantCulture);
    }

    private static string PriceToContent(OptionContract price)
    {
        if (price.Bid == price.Ask)
        {
            return price.Bid == decimal.Zero ? $"${DecimalToContent(price.Last)}" : $"${DecimalToContent(price.Bid)}";
        }

        return $"${DecimalToContent(price.Bid)} ({DecimalToContent(price.Ask)})";
    }

    private static string TupleToContent(IEnumerable<Tuple<string, string>> tuples)
    {
        var list = tuples.ToList();
        if (list.Count == 0)
        {
            return string.Empty;
        }

        var body = list
            .Select(x =>
                RenderUtils.PairToContent(RenderUtils.PropToContent(x.Item1), RenderUtils.PropToContent(x.Item2)))
            .Aggregate((curr, x) => $"{curr},{x}");

        return $"[{body}]";
    }
}