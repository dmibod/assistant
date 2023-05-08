namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;

public interface IOptionService
{
    Task<OptionChain> FindAsync(string ticker);

    Task<OptionExpiration?> FindExpirationAsync(string ticker, string expiration);
    
    Task<OptionChain> FindChangeAsync(string ticker);
    
    Task UpdateAsync(OptionChain options);
    
    Task<IEnumerable<string>> FindExpirationsAsync(string ticker);
    
    Task RemoveAsync(IDictionary<string, ISet<string>> expirations);
    
    Task RemoveAsync(string ticker);
}