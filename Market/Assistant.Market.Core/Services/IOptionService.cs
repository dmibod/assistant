namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;

public interface IOptionService
{
    Task<OptionChain> FindAsync(string ticker);
    
    Task UpdateAsync(OptionChain options);
}