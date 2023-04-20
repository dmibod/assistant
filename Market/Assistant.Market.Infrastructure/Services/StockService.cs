namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Services;
using Microsoft.Extensions.Logging;

public class StockService : IStockService
{
    private readonly ILogger<StockService> logger;

    public StockService(ILogger<StockService> logger)
    {
        this.logger = logger;
    }

    public async Task<Stock?> FindOutdatedWithLagAsync(TimeSpan lag)
    {
        this.logger.LogInformation("{Method} with lag {Argument}", nameof(this.FindOutdatedWithLagAsync), lag.ToString());

        await Task.Delay(TimeSpan.FromSeconds(1));
        
        return new Stock
        {
            Ticker = "AAPL",
            LastRefresh = DateTime.UtcNow.Subtract(lag)
        };
    }
}