namespace Assistant.Market.Core.Messaging;

using Assistant.Market.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Common.Core.Messaging.Models;
using Microsoft.Extensions.Logging;

[Handler("{StockCreateTopic}")]
public class StockCreateMessageHandler : IMessageHandler<TextMessage>
{
    private readonly IStockService stockService;
    private readonly ILogger<StockCreateMessageHandler> logger;

    public StockCreateMessageHandler(IStockService stockService, ILogger<StockCreateMessageHandler> logger)
    {
        this.stockService = stockService;
        this.logger = logger;
    }

    public Task HandleAsync(TextMessage message)
    {
        this.logger.LogInformation("Received stock create message for '{Ticker}'", message.Text);
        
        return this.stockService.GetOrCreateAsync(message.Text);
    }
}