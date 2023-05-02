namespace Assistant.Market.Core.Models;

public class Option
{
    public string Ticker { get; set; }

    public string Expiration { get; set; }

    public DateTime LastRefresh { get; set; }

    public OptionContract[] Contracts { get; set; }
}