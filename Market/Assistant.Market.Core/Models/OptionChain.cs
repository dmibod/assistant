namespace Assistant.Market.Core.Models;

using Common.Core.Utils;

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

public static class OptionChainExtensions
{
    public static OptionChain AsChain(this IEnumerable<Option> options, string ticker)
    {
        var groups = options.GroupBy(option => option.Expiration);

        return new OptionChain
        {
            Ticker = ticker,
            Expirations = groups.ToDictionary(group => group.Key, group => group.AsExpiration(group.Key))
        };
    }

    public static OptionExpiration AsExpiration(this IEnumerable<Option> options, string expiration)
    {
        var groups = options
            .SelectMany(option => option.Contracts)
            .GroupBy(contract => OptionUtils.GetStrike(contract.Ticker));
        
        return new OptionExpiration
        {
            Expiration = expiration,
            Contracts = groups.ToDictionary(group => group.Key, group => new OptionContracts
            {
                Strike = group.Key,
                Call = group.FirstOrDefault(contract => OptionUtils.GetSide(contract.Ticker) == "C"),
                Put = group.FirstOrDefault(contract => OptionUtils.GetSide(contract.Ticker) == "P")
            })
        };
    }

    public static Option AsOption(this OptionExpiration expiration, string ticker)
    {
        return new Option
        {
            Ticker = ticker,
            Expiration = expiration.Expiration,
            Contracts = expiration.Contracts.Values.SelectMany(contracts => contracts.AsEnumerable()).ToArray()
        };
    }

    public static IEnumerable<OptionContract> AsEnumerable(this OptionContracts optionContracts)
    {
        if (optionContracts.Call != null)
        {
            yield return optionContracts.Call;
        }

        if (optionContracts.Put != null)
        {
            yield return optionContracts.Put;
        }
    }
}