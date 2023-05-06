namespace Assistant.Market.Core.Messaging;

using Assistant.Market.Core.Services;
using Common.Core.Messaging;
using Microsoft.Extensions.Logging;

[Handler("{StockRefreshTopic}")]
public class StockRefreshMessageHandler : IMessageHandler<StockMessage>
{
    private readonly IRefreshService refreshService;
    private readonly ILogger<StockRefreshMessageHandler> logger;

    public StockRefreshMessageHandler(IRefreshService refreshService, ILogger<StockRefreshMessageHandler> logger)
    {
        this.refreshService = refreshService;
        this.logger = logger;
    }

    public Task HandleAsync(StockMessage message)
    {
        this.logger.LogInformation("Received stock refresh message for '{Ticker}'", message.Ticker);
        
        return this.refreshService.UpdateStockAsync(message.Ticker);
    }
}