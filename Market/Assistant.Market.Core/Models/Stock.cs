namespace Assistant.Market.Core.Models;

public class Stock : AssetPrice
{
    public DateTime LastRefresh { get; set; }
}