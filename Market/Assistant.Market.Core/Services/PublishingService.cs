﻿namespace Assistant.Market.Core.Services;

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
            await this.PublishAsync(counter++, chunk);
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
            await this.kanbanService.RemoveBoardLaneAsync(board.Id, pair.Value.Id);
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
                await this.PublishExpirationAsync(board, stockLane, optionChain, changeChain, expiration, expirationLanes);
            }

            var optionChainExpirations = optionChain.Expirations.Keys.ToHashSet();
            
            // remove expiration lanes, which are not included in chain
            foreach (var pair in expirationLanes.Where(pair => !optionChainExpirations.Contains(pair.Key)))
            {
                await this.kanbanService.RemoveBoardLaneAsync(board.Id, pair.Value.Id);
            }
        }
    }

    private async Task PublishExpirationAsync(Board board, Lane stockLane, OptionChain chain, OptionChain change, string expiration,
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

        var changeExpiration = change.Expirations.ContainsKey(expiration) ? change.Expirations[expiration] : null;
        
        var callDesc = CallsContent(chain.Expirations[expiration], changeExpiration);

        const string CallsCardName = "CALLS";

        if (!cardsMap.ContainsKey(CallsCardName))
        {
            await this.kanbanService
                .CreateCardAsync(board.Id, expirationLane.Id, new Card { Name = CallsCardName, Description = callDesc });
        }
        else
        {
            cardsMap[CallsCardName].Description = callDesc;

            await this.kanbanService.UpdateCardAsync(board.Id, cardsMap[CallsCardName]);
        }

        var putDesc = PutsContent(chain.Expirations[expiration], changeExpiration);

        const string PutsCardName = "PUTS";

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
        var tuples = expiration.Contracts
            .Where(pair => pair.Value.Call != null)
            .OrderBy(pair => pair.Key)
            .Select(pair => new Tuple<string, Tuple<string, IDictionary<string, string>?>>(DecimalToContent(pair.Key), PriceToContent(pair.Value.Call, GetCallContract(pair.Key, change))));

        return TupleToContent(tuples);
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
        var tuples = expiration.Contracts
            .Where(pair => pair.Value.Put != null)
            .OrderBy(pair => pair.Key)
            .Select(pair => new Tuple<string, Tuple<string, IDictionary<string, string>?>>(DecimalToContent(pair.Key), PriceToContent(pair.Value.Put, GetPutContract(pair.Key, change))));

        return TupleToContent(tuples);
    }

    private static string DecimalToContent(decimal? value)
    {
        var round = Math.Round(value ?? decimal.Zero, 2);

        return round.ToString(CultureInfo.InvariantCulture);
    }

    private static Tuple<string, IDictionary<string, string>?> PriceToContent(OptionContract price, OptionContract? contract)
    {
        var content = price.Bid == price.Ask 
            ? price.Bid == decimal.Zero ? $"${DecimalToContent(price.Last)}" : $"${DecimalToContent(price.Bid)}" 
            : $"${DecimalToContent(price.Bid)} ({DecimalToContent(price.Ask)})";

        if (contract != null && contract.OI != decimal.Zero)
        {
            content += $" [{Math.Round(Math.Abs(contract.OI), 0).ToString(CultureInfo.InvariantCulture)}]";

            return new Tuple<string, IDictionary<string, string>?>(content,
                contract.OI < decimal.Zero ? RenderUtils.RedStyle : RenderUtils.GreenStyle);
        }

        return new Tuple<string, IDictionary<string, string>?>(content, null);
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
                RenderUtils.PairToContent(RenderUtils.PropToContent(x.Item1), RenderUtils.PropToContent(x.Item2.Item1, x.Item2.Item2)))
            .Aggregate((curr, x) => $"{curr},{x}");

        return $"[{body}]";
    }
}