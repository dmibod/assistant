namespace Helper.Core.Domain;

public class MarketPrice
{
    public MarketPrice() : this(decimal.Zero, decimal.Zero)
    {
    }

    private MarketPrice(decimal bid, decimal ask)
    {
        this.Bid = bid;
        this.Ask = ask;
    }

    public static MarketPrice From(decimal bid, decimal ask)
    {
        return new MarketPrice(bid, ask);
    }

    public static MarketPrice From(decimal price) => From(price, price);

    public decimal Bid { get; }
    
    public decimal Ask { get; }
}