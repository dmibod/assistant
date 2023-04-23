namespace Helper.Core.Domain;

using Helper.Core.Specification;

public class OptionChain
{
    private OptionChain(Stock stock, Expiration expiration, IEnumerable<decimal> strikes)
    {
        this.Stock = stock;
        this.Expiration = expiration;
        this.Strikes = strikes;
    }

    public static OptionChain For(Stock stock, Expiration expiration, IEnumerable<decimal> strikes)
    {
        return new OptionChain(stock, expiration, strikes);
    }

    public Stock Stock { get; }

    public Expiration Expiration { get; }

    public IEnumerable<decimal> Strikes { get; }

    public StockOption GetOption(OptionType optionType, decimal strike)
    {
        return optionType == OptionType.Call 
                ? StockOption.Call(this.Stock, strike, this.Expiration) 
                : StockOption.Put(this.Stock, strike, this.Expiration);
    }

    public IEnumerable<StockOption> FindOptions(ISpecification<StockOption> specification)
    {
        foreach (var strike in this.Strikes)
        {
            var callOption = this.GetCall(strike);
            
            if (specification.IsSatisfied(callOption))
            {
                yield return callOption;
            }
            
            var putOption = this.GetPut(strike);
            
            if (specification.IsSatisfied(putOption))
            {
                yield return putOption;
            }
        }
    }

    public StockOption GetCall(decimal strike)
    {
        return this.GetOption(OptionType.Call, strike);
    }
    
    public StockOption GetPut(decimal strike)
    {
        return this.GetOption(OptionType.Put, strike);
    }
}