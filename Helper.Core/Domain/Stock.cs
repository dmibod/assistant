namespace Helper.Core.Domain;

public class Stock : Asset<string>
{
    private Stock(string ticker, IEnumerable<Expiration>? expirations): base(ticker)
    {
        this.Expirations = expirations;
    }

    public static Stock From(string ticker, IQueryable<Expiration>? expirations = null)
    {
        return new Stock(ticker, expirations);
    }

    public IEnumerable<Expiration>? Expirations { get; }

    public virtual ushort GetOptionContractSize()
    {
        return 100;
    }
}