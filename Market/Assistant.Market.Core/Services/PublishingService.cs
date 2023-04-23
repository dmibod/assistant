namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;
using Microsoft.Extensions.Logging;
using Exception = System.Exception;

public class PublishingService : IPublishingService
{
    private const string MarketData = "Market Data";
    private const int ChunkSize = 10;

    private readonly IStockService stockService;
    private readonly IKanbanService kanbanService;
    private readonly ILogger<PublishingService> logger;

    public PublishingService(IStockService stockService, IKanbanService kanbanService, ILogger<PublishingService> logger)
    {
        this.stockService = stockService;
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
        var key = $"{MarketData}#{chunkNo}";
        var name = $"{key} ({chunk.Length})";
        var description = chunk.Select(item => item.Ticker).Aggregate((curr, i) => $"{curr}, {i}");

        var boards = this.kanbanService.FindBoardsAsync().Result
            .Where(board => board.Name.StartsWith(MarketData))
            .ToDictionary(board => board.Name);

        if (boards.ContainsKey(key))
        {
            var board = boards[key];
            board.Name = name;
            board.Description = description;
            
            this.kanbanService.UpdateBoardAsync(board).GetAwaiter().GetResult();
        }
        else
        {
            var board = this.kanbanService.CreateBoardAsync(new Board
            {
                Name = name,
                Description = description
            }).Result;
            
            boards.Add(key, board);
        }
    }

    private void Publish(Board board, Stock[] chunk)
    {
        try
        {
            this.kanbanService.SetBoardLoadingStateAsync(board.Id).GetAwaiter().GetResult();

            var laneMap = PublishTickers(board, chunk);
            PublishExpirations(board, chunk, laneMap);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to publish marked data for {Board} with {Content}", board.Name, board.Description);
        }
        finally
        {
            this.kanbanService.ResetBoardStateAsync(board.Id).GetAwaiter().GetResult();
        }
    }

    private IDictionary<string, Lane> PublishTickers(Board board, Stock[] chunk)
    {
        var laneMap = this.kanbanService
            .FindBoardLanesAsync(board.Id)
            .Result
            .ToDictionary(lane => lane.Name, lane => lane);

        foreach (var stock in chunk)
        {
            var description = stock.LastRefresh != DateTime.UnixEpoch
                ? $"${Math.Round(stock.Last ?? decimal.Zero, 2)}, {stock.LastRefresh.ToShortDateString()} {stock.LastRefresh.ToShortTimeString()}"
                : "n/a";

            if (!laneMap.ContainsKey(stock.Ticker))
            {
                var lane = this.kanbanService
                    .CreateBoardLaneAsync(board.Id, new Lane{ Name = stock.Ticker, Description = description})
                    .Result;

                laneMap.Add(stock.Ticker, lane);
            }
            else
            {
                laneMap[stock.Ticker].Description = description;
                
                this.kanbanService
                    .UpdateBoardLaneAsync(board.Id, laneMap[stock.Ticker])
                    .GetAwaiter()
                    .GetResult();
            }
        }

        var map = chunk.ToDictionary(i => i.Ticker);
        
        laneMap.Keys.ToList().ForEach(ticker =>
        {
            if (!map.ContainsKey(ticker))
            {
                this.kanbanService.RemoveBoardLaneAsync(board.Id, laneMap[ticker].Id).GetAwaiter().GetResult();
            }
        });

        return laneMap;
    }

    private void PublishExpirations(Board board, Stock[] chunk, IDictionary<string, Lane> laneMap)
    {
    }
}