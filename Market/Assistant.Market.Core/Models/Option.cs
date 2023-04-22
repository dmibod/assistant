namespace Assistant.Market.Core.Models;

public class Option
{
    public string Ticker { get; set; }

    public string Expiration { get; set; }

    public OptionContract[] Contracts { get; set; }
}