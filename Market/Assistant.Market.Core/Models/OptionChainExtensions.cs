namespace Assistant.Market.Core.Models;

using Helper.Core.Utils;

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

        var option = options.FirstOrDefault(o => o.Expiration == expiration);
        
        return new OptionExpiration
        {
            Expiration = expiration,
            LastRefresh = option == null ? DateTime.UnixEpoch : option.LastRefresh,
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