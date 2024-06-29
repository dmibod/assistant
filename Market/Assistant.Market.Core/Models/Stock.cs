namespace Assistant.Market.Core.Models;

public class Stock : AssetPrice
{
    public double MarketCap { get; set; }
    
    public DateTime LastRefresh { get; set; }
}