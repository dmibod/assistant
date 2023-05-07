namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Repositories;
using Helper.Core.Domain;
using Helper.Core.Utils;
using Microsoft.Extensions.Logging;

public class WatchListService : IWatchListService
{
    private readonly ITenantService tenantService;
    private readonly ITenantRepository repository;
    private readonly IMarketDataService marketDataService;
    private readonly ILogger<WatchListService> logger;

    public WatchListService(ITenantService tenantService, ITenantRepository repository,
        IMarketDataService marketDataService, ILogger<WatchListService> logger)
    {
        this.tenantService = tenantService;
        this.repository = repository;
        this.marketDataService = marketDataService;
        this.logger = logger;
    }

    public async Task<IEnumerable<WatchListItem>> FindAllAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindAllAsync));

        var tenant = await this.tenantService.EnsureExistsAsync();

        return await this.repository.FindWatchListAsync(tenant);
    }

    public async Task<WatchListItem?> FindByTickerAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindByTickerAsync), ticker);

        ticker = StockUtils.Format(ticker);
        
        var tenant = await this.tenantService.EnsureExistsAsync();

        var watchList = await this.repository.FindWatchListAsync(tenant);
        
        return watchList.FirstOrDefault(item => item.Ticker == ticker);
    }

    public async Task<WatchListItem> CreateOrUpdateAsync(WatchListItem listItem, bool ignoreIfExists)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateOrUpdateAsync), listItem.Ticker);

        listItem.Ticker = StockUtils.Format(listItem.Ticker);
        
        var tenant = await this.tenantService.EnsureExistsAsync();

        var watchList = await this.repository.FindWatchListAsync(tenant);

        var existingItem = watchList.FirstOrDefault(item => item.Ticker == listItem.Ticker);

        if (existingItem == null)
        {
            await this.repository.CreateWatchListItemAsync(tenant, listItem);

            await this.marketDataService.EnsureStockAsync(listItem.Ticker);
        }
        else if (!ignoreIfExists)
        {
            await this.repository.SetWatchListItemPricesAsync(tenant, listItem.Ticker, listItem.BuyPrice, listItem.SellPrice);
        }

        return listItem;
    }

    public async Task RemoveAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveAsync), ticker);

        ticker = StockUtils.Format(ticker);
        
        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.RemoveWatchListItemAsync(tenant, ticker);
    }

    public async Task SetBuyPriceAsync(string ticker, decimal price)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetBuyPriceAsync),
            $"{ticker}-{price}");
        
        ticker = StockUtils.Format(ticker);

        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.SetWatchListItemBuyPriceAsync(tenant, ticker, price);
    }

    public async Task SetSellPriceAsync(string ticker, decimal price)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetSellPriceAsync),
            $"{ticker}-{price}");
        
        ticker = StockUtils.Format(ticker);

        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.SetWatchListItemSellPriceAsync(tenant, ticker, price);
    }

    public async Task SetPricesAsync(string ticker, decimal buyPrice, decimal sellPrice)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetPricesAsync),
            $"{ticker}-{buyPrice}-{sellPrice}");
        
        ticker = StockUtils.Format(ticker);

        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.SetWatchListItemPricesAsync(tenant, ticker, buyPrice, sellPrice);
    }
}