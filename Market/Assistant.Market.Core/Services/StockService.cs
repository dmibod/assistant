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

        ticker = ticker.ToUpper();
        
        if (!await this.repository.ExistsAsync(ticker))
        {
            await this.repository.CreateAsync(new Stock
            {
                Ticker = ticker,
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
        
        stock.LastRefresh = DateTime.UtcNow;

        await this.repository.UpdateAsync(stock);
    }

    public Task<string?> FindOutdatedTickerAsync(TimeSpan olderThan)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindOutdatedTickerAsync),
            olderThan.ToString());

        return this.repository.FindOutdatedTickerAsync(olderThan);
    }

    public Task<IEnumerable<Stock>> FindAllAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindAllAsync));

        return this.repository.FindAllAsync();
    }

    public Task<IEnumerable<string>> FindTickersAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindTickersAsync));

        return this.repository.FindTickersAsync();
    }

    public Task<Stock?> FindByTickerAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindByTickerAsync), ticker);

        ticker = ticker.ToUpper();
        
        return this.repository.FindByTickerAsync(ticker);
    }
}