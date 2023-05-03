namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;

public interface IOptionService
{
    Task<OptionChain> FindAsync(string ticker);
    
    Task<OptionChain> FindChangeAsync(string ticker);
    
    Task UpdateAsync(OptionChain options);
    
    Task<IEnumerable<string>> FindExpirationsAsync(string ticker);
    
    Task RemoveAsync(IDictionary<string, ISet<string>> expirations);
}