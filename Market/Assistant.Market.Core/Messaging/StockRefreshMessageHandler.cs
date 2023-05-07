namespace Assistant.Market.Core.Messaging;

using Assistant.Market.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Microsoft.Extensions.Logging;

[Handler("{StockRefreshTopic}")]
public class StockRefreshMessageHandler : IMessageHandler<StockRefreshMessage>
{
    private readonly IRefreshService refreshService;
    private readonly ILogger<StockRefreshMessageHandler> logger;

    public StockRefreshMessageHandler(IRefreshService refreshService, ILogger<StockRefreshMessageHandler> logger)
    {
        this.refreshService = refreshService;
        this.logger = logger;
    }

    public Task HandleAsync(StockRefreshMessage message)
    {
        this.logger.LogInformation("Received stock refresh message for '{Ticker}'", message.Ticker);
        
        return this.refreshService.UpdateStockAsync(message.Ticker);
    }
}

public class StockRefreshMessage
{
    public string Ticker { get; set; }
}