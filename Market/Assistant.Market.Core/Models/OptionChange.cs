namespace Assistant.Market.Core.Models;

public class OptionChange
{
    public string OptionTicker { get; set; }

    public decimal OpenInterestChange { get; set; }

    public decimal OpenInterestChangePercent { get; set; }
    
    public decimal Bid { get; set; }
    
    public decimal Ask { get; set; }
    
    public decimal Last { get; set; }
}