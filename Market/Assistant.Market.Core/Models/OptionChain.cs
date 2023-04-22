namespace Assistant.Market.Core.Models;

public class OptionChain
{
    public string Ticker { get; set; }

    public IDictionary<string, OptionExpiration> Expirations { get; set; }
}

public class OptionExpiration
{
    public string Expiration { get; set; }
    
    public IDictionary<decimal, OptionContracts> Contracts { get; set; }
}

public class OptionContracts
{
    public decimal Strike { get; set; }

    public OptionContract Call { get; set; }
    
    public OptionContract Put { get; set; }
}

public class OptionContract
{
    public string Ticker { get; set; }
    
    public decimal? Bid { get; set; }
    
    public decimal? Ask { get; set; }
    
    public decimal? Last { get; set; }
}