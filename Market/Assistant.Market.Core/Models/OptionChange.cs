namespace Assistant.Market.Core.Models;

public class OptionChange
{
    public string OptionTicker { get; set; }

    public decimal OpenInterestChange { get; set; }

    public decimal OpenInterestChangePercent { get; set; }
}