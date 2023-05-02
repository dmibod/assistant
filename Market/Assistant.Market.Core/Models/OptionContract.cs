namespace Assistant.Market.Core.Models;

public class OptionContract : AssetPrice
{
    public decimal Vol { get; set; }
    
    public decimal OI { get; set; }
}