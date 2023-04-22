namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;

public interface IMarketDataService
{
    Task<AssetPrice?> GetStockPriceAsync(string ticker);
    
    Task<OptionChain?> GetOptionChainAsync(string ticker);
}

public class AssetPrice
{
    public string Ticker { get; set; }

    public decimal Bid { get; set; }
    
    public decimal Ask { get; set; }
    
    public decimal Last { get; set; }
        
    public DateTime TimeStamp { get; set; }
}