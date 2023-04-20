namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

public class StockService : IStockService
{
    private readonly StockRepository repository;
    private readonly ILogger<StockService> logger;

    public StockService(StockRepository repository, ILogger<StockService> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Stock?> FindOutdatedWithLagAsync(TimeSpan lag)
    {
        this.logger.LogInformation("{Method} with lag {Argument}", nameof(this.FindOutdatedWithLagAsync),
            lag.ToString());

        return this.repository.FindOutdatedWithLagAsync(lag);
    }
}