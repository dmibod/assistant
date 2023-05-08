namespace Assistant.Market.Core.Repositories;

using Assistant.Market.Core.Models;

public interface IOptionChangeRepository
{
    Task<IEnumerable<Option>> FindByTickerAsync(string ticker);

    Task CreateOrUpdateAsync(Option option);

    Task RemoveAsync(IDictionary<string, ISet<string>> expirations);
    
    Task RemoveAsync(string ticker);
}