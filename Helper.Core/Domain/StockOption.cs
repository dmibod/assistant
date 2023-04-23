namespace Helper.Core.Domain;

public class StockOption : Asset<StockOptionId>
{
    private StockOption(StockOptionId id, Stock stock) : base(id)
    {
        this.Stock = stock;
    }

    public static StockOption Call(Stock stock, decimal strike, Expiration expiration)
    {
        return new StockOption(StockOptionId.CallFrom(stock.Id, strike, expiration), stock);
    }

    public static StockOption Put(Stock stock, decimal strike, Expiration expiration)
    {
        return new StockOption(StockOptionId.PutFrom(stock.Id, strike, expiration), stock);
    }
    
    public Stock Stock { get; }

    public ushort DaysTillExpiration => this.Id.Expiration.DaysTillExpiration;
    
    public decimal Collateral => this.Id.Strike * this.Stock.GetOptionContractSize();
    
    public bool InTheMoney => this.Id.OptionType == OptionType.Call 
        ? this.Stock.BuyPrice > this.Id.Strike 
        : this.Stock.SellPrice < this.Id.Strike;
    
    public SellOperation Sell(decimal price)
    {
        return new SellOperation(this, price);
    }
}