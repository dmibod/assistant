namespace Assistant.Market.Core.Messaging;

using Assistant.Market.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Microsoft.Extensions.Logging;

[Handler("{StockCreateTopic}")]
public class StockCreateMessageHandler : IMessageHandler<StockCreateMessage>
{
    private readonly IStockService stockService;
    private readonly ILogger<StockCreateMessageHandler> logger;

    public StockCreateMessageHandler(IStockService stockService, ILogger<StockCreateMessageHandler> logger)
    {
        this.stockService = stockService;
        this.logger = logger;
    }

    public Task HandleAsync(StockCreateMessage message)
    {
        this.logger.LogInformation("Received stock create message for '{Ticker}'", message.Ticker);
        
        return this.stockService.GetOrCreateAsync(message.Ticker);
    }
}

public class StockCreateMessage
{
    public string Ticker { get; set; }
} 