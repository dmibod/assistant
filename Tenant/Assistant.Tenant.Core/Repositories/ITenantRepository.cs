﻿namespace Assistant.Tenant.Core.Repositories;

using Assistant.Tenant.Core.Models;

public interface ITenantRepository
{
    Task<Tenant> FindByNameAsync(string name);

    Task<bool> ExistsAsync(string name);
    
    Task CreateAsync(string name);

    Task CreatePositionAsync(string tenant, Position position);

    Task RemovePositionAsync(string tenant, string account, string ticker);
    
    Task TagPositionAsync(string tenant, string account, string ticker, string tag);
    
    Task CreateWatchListItemAsync(string tenant, WatchListItem listItem);
    
    Task RemoveWatchListItemAsync(string tenant, string ticker);

    Task SetWatchListItemBuyPriceAsync(string tenant, string ticker, decimal price);
    
    Task SetWatchListItemSellPriceAsync(string tenant, string ticker, decimal price);
    
    Task SetWatchListItemPricesAsync(string tenant, string ticker, decimal buyPrice, decimal sellPrice);
}