namespace Assistant.Market.Core.Repositories;

using Assistant.Market.Core.Models;

public interface IOptionRepository
{
    Task<IEnumerable<Option>> FindByTickerAsync(string ticker);
    
    Task<IEnumerable<string>> FindExpirationsAsync(string ticker);
    
    Task<bool> ExistsAsync(string ticker, string expiration);

    Task<Option?> FindExpirationAsync(string ticker, string expiration);
    
    Task CreateAsync(Option option);

    Task UpdateAsync(Option option);
    
    Task RemoveAsync(IDictionary<string, ISet<string>> expirations);
}