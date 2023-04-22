namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Repositories;
using Microsoft.Extensions.Logging;

public class StockService : IStockService
{
    private readonly IStockRepository repository;
    private readonly ILogger<StockService> logger;

    public StockService(IStockRepository repository, ILogger<StockService> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Stock> GetOrCreateAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.GetOrCreateAsync), ticker);
        
        if (!await this.repository.ExistsAsync(ticker))
        {
            await this.repository.CreateAsync(new Stock
            {
                Ticker = ticker.ToUpper(),
                LastRefresh = DateTime.UnixEpoch
            });
        }

        return await this.repository.FindByTickerAsync(ticker);
    }

    public async Task UpdateAsync(Stock stock)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateAsync), stock.Ticker);

        if (!await this.repository.ExistsAsync(stock.Ticker))
        {
            throw new Exception($"Stock with ticker '{stock.Ticker}' is not found.");
        }
        
        await this.repository.UpdateAsync(stock);
    }

    public Task<Stock?> FindOldestAsync(TimeSpan olderThan)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindOldestAsync),
            olderThan.ToString());

        return this.repository.FindOldestAsync(olderThan);
    }
}