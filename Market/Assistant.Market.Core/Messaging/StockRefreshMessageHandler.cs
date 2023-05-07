namespace Assistant.Market.Core.Messaging;

using Assistant.Market.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Common.Core.Messaging.Models;
using Microsoft.Extensions.Logging;

[Handler("{StockRefreshTopic}")]
public class StockRefreshMessageHandler : IMessageHandler<TextMessage>
{
    private readonly IRefreshService refreshService;
    private readonly ILogger<StockRefreshMessageHandler> logger;

    public StockRefreshMessageHandler(IRefreshService refreshService, ILogger<StockRefreshMessageHandler> logger)
    {
        this.refreshService = refreshService;
        this.logger = logger;
    }

    public Task HandleAsync(TextMessage message)
    {
        this.logger.LogInformation("Received stock refresh message for '{Ticker}'", message.Text);
        
        return this.refreshService.UpdateStockAsync(message.Text);
    }
}