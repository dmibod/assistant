namespace Assistant.Tenant.Core.Services;

public interface IMarketDataService
{
    Task EnsureStockAsync(string ticker);
    
    Task<IEnumerable<AssetPrice>> FindStockPricesAsync(ISet<string> tickers);
    
    Task<IEnumerable<OptionAssetPrice>> FindOptionPricesAsync(string stockTicker, string expiration);

    Task<IEnumerable<string>> FindExpirationsAsync(string ticker);
}

public class AssetPrice
{
    public string Ticker { get; set; }
    
    public decimal? Bid { get; set; }

    public decimal? Ask { get; set; }

    public decimal? Last { get; set; }
    
    public DateTime TimeStamp { get; set; }
}

public class OptionAssetPrice : AssetPrice
{
    public decimal? Vol { get; set; }
    
    public decimal? OI { get; set; }
}

public class OptionPrice
{
    public string Ticker { get; set; }

    public string Expiration { get; set; }

    public OptionAssetPrice[] Contracts { get; set; }
}
