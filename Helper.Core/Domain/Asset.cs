namespace Helper.Core.Domain;

public abstract class Asset<T> where T : notnull
{
    protected Asset(T id)
    {
        this.Id = id;
    }

    public T Id { get; }

    public MarketPrice Price { get; set; }

    public decimal SellPrice => this.Price.Bid;

    public decimal BuyPrice => this.Price.Ask;
}