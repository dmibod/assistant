﻿namespace Assistant.Tenant.Core.Services;

public interface IMarketDataService
{
    Task<IEnumerable<AssetPrice>> FindStockPricesAsync();
    
    Task<IEnumerable<AssetPrice>> FindOptionPricesAsync(string stockTicker, string expiration);
}

public class AssetPrice
{
    public string Ticker { get; set; }
    
    public decimal? Bid { get; set; }

    public decimal? Ask { get; set; }

    public decimal? Last { get; set; }
}

public class OptionPrice
{
    public string Ticker { get; set; }

    public string Expiration { get; set; }

    public AssetPrice[] Contracts { get; set; }
}
