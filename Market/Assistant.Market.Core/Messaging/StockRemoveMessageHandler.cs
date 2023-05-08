namespace Assistant.Market.Core.Messaging;

using Assistant.Market.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Microsoft.Extensions.Logging;

[Handler("{StockRemoveTopic}")]
public class StockRemoveMessageHandler : IMessageHandler<StockRemoveMessage>
{
    private readonly IStockService stockService;
    private readonly IOptionService optionService;
    private readonly ILogger<StockRemoveMessageHandler> logger;

    public StockRemoveMessageHandler(IStockService stockService, IOptionService optionService, ILogger<StockRemoveMessageHandler> logger)
    {
        this.stockService = stockService;
        this.optionService = optionService;
        this.logger = logger;
    }

    public async Task HandleAsync(StockRemoveMessage message)
    {
        this.logger.LogInformation("Received stock remove message for '{Ticker}'", message.Ticker);

        await this.optionService.RemoveAsync(message.Ticker);
        
        await this.stockService.RemoveAsync(message.Ticker);
    }
}

public class StockRemoveMessage
{
    public string Ticker { get; set; }
} 