﻿namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Repositories;
using Microsoft.Extensions.Logging;

public class WatchListService : IWatchListService
{
    private readonly ITenantService tenantService;
    private readonly ITenantRepository repository;
    private readonly IMarketDataService marketDataService;
    private readonly ILogger<WatchListService> logger;

    public WatchListService(ITenantService tenantService, ITenantRepository repository, IMarketDataService marketDataService, ILogger<WatchListService> logger)
    {
        this.tenantService = tenantService;
        this.repository = repository;
        this.marketDataService = marketDataService;
        this.logger = logger;
    }

    public async Task<IEnumerable<WatchListItem>> FindAllAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindAllAsync));
        
        var tenant = await this.tenantService.GetOrCreateAsync();

        return tenant.WatchList;
    }

    public async Task<WatchListItem> CreateAsync(WatchListItem listItem)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateAsync), listItem.Ticker);
        
        var tenant = await this.tenantService.GetOrCreateAsync();

        await this.repository.CreateWatchListItemAsync(tenant.Name, listItem);

            await this.marketDataService.EnsureStockAsync(listItem.Ticker);

        return listItem;
    }

    public async Task RemoveAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveAsync), ticker);

        var tenant = await this.tenantService.GetOrCreateAsync();
        
        await this.repository.RemoveWatchListItemAsync(tenant.Name, ticker);
    }

    public async Task SetBuyPriceAsync(string ticker, decimal price)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetBuyPriceAsync), $"{ticker}-{price}");

        var tenant = await this.tenantService.GetOrCreateAsync();
        
        await this.repository.SetWatchListItemBuyPriceAsync(tenant.Name, ticker, price);
    }

    public async Task SetSellPriceAsync(string ticker, decimal price)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetSellPriceAsync), $"{ticker}-{price}");

        var tenant = await this.tenantService.GetOrCreateAsync();
        
        await this.repository.SetWatchListItemSellPriceAsync(tenant.Name, ticker, price);
    }

    public async Task SetPricesAsync(string ticker, decimal buyPrice, decimal sellPrice)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetPricesAsync), $"{ticker}-{buyPrice}-{sellPrice}");

        var tenant = await this.tenantService.GetOrCreateAsync();
        
        await this.repository.SetWatchListItemPricesAsync(tenant.Name, ticker, buyPrice, sellPrice);
    }
}