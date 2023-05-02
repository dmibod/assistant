namespace Assistant.Market.Core.Models;

public class OptionChain
{
    public string Ticker { get; set; }

    public IDictionary<string, OptionExpiration> Expirations { get; set; }
}