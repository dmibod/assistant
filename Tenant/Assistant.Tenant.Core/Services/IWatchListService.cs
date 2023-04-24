namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface IWatchListService
{
    Task<WatchListItem> CreateAsync(WatchListItem listItem);

    Task RemoveAsync(string ticker);

    Task SetBuyPriceAsync(string ticker, decimal price);

    Task SetSellPriceAsync(string ticker, decimal price);

    Task SetPricesAsync(string ticker, decimal buyPrice, decimal sellPrice);
}