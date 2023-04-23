namespace Assistant.Market.Core.Repositories;

using Assistant.Market.Core.Models;

public interface IOptionRepository
{
    Task<bool> ExistsAsync(string ticker, string expiration);
    
    Task UpdateAsync(Option option);
    
    Task CreateAsync(Option option);
    
    Task<IEnumerable<Option>> FindByTickerAsync(string ticker);
    
    Task<IEnumerable<string>> FindExpirationsAsync(string ticker);
    
    Task RemoveAsync(IDictionary<string, ISet<string>> expirations);
}