namespace Assistant.Market.Core.Messaging;

using Assistant.Market.Core.Services;
using Common.Core.Messaging;
using Microsoft.Extensions.Logging;

[Handler("{StockCreateTopic}")]
public class StockCreateMessageHandler : IMessageHandler<StockMessage>
{
    private readonly IStockService stockService;
    private readonly ILogger<StockCreateMessageHandler> logger;

    public StockCreateMessageHandler(IStockService stockService, ILogger<StockCreateMessageHandler> logger)
    {
        this.stockService = stockService;
        this.logger = logger;
    }

    public Task HandleAsync(StockMessage message)
    {
        this.logger.LogInformation("Received stock create message for '{Ticker}'", message.Ticker);
        
        return this.stockService.GetOrCreateAsync(message.Ticker);
    }
}