namespace Assistant.Market.Core.Models;

public class OptionExpiration
{
    public string Expiration { get; set; }
    
    public DateTime LastRefresh { get; set; }

    public IDictionary<decimal, OptionContracts> Contracts { get; set; }
}