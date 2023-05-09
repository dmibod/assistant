namespace Assistant.Tenant.Core.Models;

public class AssetPrice
{
    public string Ticker { get; set; }
    
    public decimal? Bid { get; set; }

    public decimal? Ask { get; set; }

    public decimal? Last { get; set; }
    
    public DateTime TimeStamp { get; set; }
}