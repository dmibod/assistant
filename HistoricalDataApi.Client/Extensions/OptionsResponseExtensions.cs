namespace HistoricalDataApi.Client.Extensions;

public static class OptionsResponseExtensions
{
    public static IEnumerable<OptionContractData> GetContracts(this OptionExpirationData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }
        
        var empty = Array.Empty<OptionContractData>();
        var calls = data.Options == null ? empty : data.Options.Calls ?? empty;
        var puts = data.Options == null ? empty : data.Options.Puts ?? empty;

        return calls.Union(puts);
    }
}