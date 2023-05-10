namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface IMarketDataService
{
    Task EnsureStockAsync(string ticker);
    
    Task<IEnumerable<AssetPrice>> FindStockPricesAsync(ISet<string> tickers);
    
    Task<IEnumerable<OptionAssetPrice>> FindOptionPricesAsync(string stockTicker, string expiration);

    Task<IEnumerable<OptionAssetPrice>> FindOptionPricesChangeAsync(string stockTicker, string expiration);
    
    Task<IEnumerable<OptionAssetPrice>> FindOptionPricesChangeSinceAsync(string stockTicker, string expiration, DateTime since);

    Task<IEnumerable<string>> FindExpirationsAsync(string stockTicker);
}

public class OptionPrice
{
    public string Ticker { get; set; }

    public string Expiration { get; set; }

    public OptionAssetPrice[] Contracts { get; set; }
}
