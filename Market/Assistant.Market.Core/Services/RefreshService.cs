namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Utils;
using Microsoft.Extensions.Logging;

public class RefreshService : IRefreshService
{
    private readonly IMarketDataService marketDataService;
    private readonly IStockService stockService;
    private readonly IOptionService optionService;
    private readonly ILogger<RefreshService> logger;

    public RefreshService(IMarketDataService marketDataService, IStockService stockService, IOptionService optionService, ILogger<RefreshService> logger)
    {
        this.marketDataService = marketDataService;
        this.stockService = stockService;
        this.optionService = optionService;
        this.logger = logger;
    }

    public async Task RefreshAsync(TimeSpan lag)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RefreshAsync), lag.ToString());

        var stock = await this.stockService.FindOldestAsync(lag);
        if (stock != null)
        {
            await this.UpdateStockAsync(stock);
        }
    }

    public async Task CleanAsync(DateTime now)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CleanAsync), now.ToShortDateString());

        var threshold = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        var tickers = await this.stockService.FindTickersAsync();
        var outdated = new Dictionary<string, ISet<string>>();

        foreach (var ticker in tickers)
        {
            var expirations = await this.optionService.FindExpirationsAsync(ticker);
                
            var outdatedExpirations = expirations.Where(expiration => OptionUtils.ParseExpiration(expiration) < threshold).ToHashSet();

            if (outdatedExpirations.Count > 0)
            {
                outdated.Add(ticker, outdatedExpirations);
            }
        }

        if (outdated.Count > 0)
        {
            await this.optionService.RemoveAsync(outdated);
        }
    }

    private async Task UpdateStockAsync(Stock stock)
    {
        var stockPrice = await this.marketDataService.GetStockPriceAsync(stock.Ticker);

        if (stockPrice != null)
        {
            stock.Ask = stockPrice.Ask;
            stock.Bid = stockPrice.Bid;
            stock.Last = stockPrice.Last;
            stock.LastRefresh = DateTime.UtcNow;
            
            await this.stockService.UpdateAsync(stock);
        }

        var optionChain = await this.marketDataService.GetOptionChainAsync(stock.Ticker);

        if (optionChain != null)
        {
            await this.optionService.UpdateAsync(optionChain);
        }
    }
}