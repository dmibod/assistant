namespace Helper.Core.Domain;

public struct StockOptionId
{
    private StockOptionId(string ticker, decimal strike, OptionType optionType, Expiration expiration)
    {
        this.Ticker = ticker;
        this.Strike = strike;
        this.OptionType = optionType;
        this.Expiration = expiration;
    }

    public static StockOptionId From(string ticker, decimal strike, OptionType optionType, Expiration expiration) => new StockOptionId(ticker, strike, optionType, expiration);

    public static StockOptionId FromCurrentYear(string ticker, decimal strike, OptionType optionType, byte day, Months month) => From(ticker, strike, optionType, Expiration.FromCurrentYear(day, month));
    
    public static StockOptionId FromCurrentMonth(string ticker, decimal strike, OptionType optionType) => From(ticker, strike, optionType, Expiration.Now);

    public static StockOptionId CallFrom(string ticker, decimal strike, Expiration expiration) => new StockOptionId(ticker, strike, OptionType.Call, expiration);

    public static StockOptionId CallFromCurrentYear(string ticker, decimal strike, byte day, Months month) => CallFrom(ticker, strike, Expiration.FromCurrentYear(day, month));
    
    public static StockOptionId CallFromCurrentMonth(string ticker, decimal strike) => CallFrom(ticker, strike, Expiration.Now);

    public static StockOptionId PutFrom(string ticker, decimal strike, Expiration expiration) => new StockOptionId(ticker, strike, OptionType.Put, expiration);

    public static StockOptionId PutFromCurrentYear(string ticker, decimal strike, byte day, Months month) => PutFrom(ticker, strike, Expiration.FromCurrentYear(day, month));
    
    public static StockOptionId PutFromCurrentMonth(string ticker, decimal strike) => PutFrom(ticker, strike, Expiration.Now);

    public string Ticker { get; }

    public decimal Strike { get; }
    
    public OptionType OptionType { get; }
    
    public Expiration Expiration { get; }
}