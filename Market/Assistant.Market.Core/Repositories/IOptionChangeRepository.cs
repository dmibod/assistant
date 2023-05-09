namespace Assistant.Market.Core.Repositories;

using Assistant.Market.Core.Models;

public interface IOptionChangeRepository
{
    Task<IEnumerable<Option>> FindByTickerAsync(string ticker);

    Task CreateOrUpdateAsync(Option option);

    Task RemoveAsync(IDictionary<string, ISet<string>> expirations);
    
    Task RemoveAsync(string ticker);
    
    Task<int> FindChangesCountAsync(string ticker);
    
    Task<decimal> FindOpenInterestMinAsync(string ticker);
    
    Task<decimal> FindOpenInterestMaxAsync(string ticker);
    
    Task<decimal> FindOpenInterestPercentMinAsync(string ticker);
    
    Task<decimal> FindOpenInterestPercentMaxAsync(string ticker);

}