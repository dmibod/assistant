namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;
using Microsoft.Extensions.Logging;

public class PublishingService : IPublishingService
{
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
            this.Publish(counter++, chunk, map);
        }
    }

    private Task Publish(int chunkNo, IEnumerable<Stock> chunk, IDictionary<string, Stock> stocks)
    {
        return Task.CompletedTask;
    }
}