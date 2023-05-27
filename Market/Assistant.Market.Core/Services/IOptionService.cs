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
    
    Task<int> FindChangesCountAsync(string ticker, Func<DateTime> todayFn);

    Task<decimal> FindOpenInterestChangeMinAsync(string ticker, Func<DateTime> todayFn);
    
    Task<decimal> FindOpenInterestChangeMaxAsync(string ticker, Func<DateTime> todayFn);
    
    Task<decimal> FindOpenInterestChangePercentMinAsync(string ticker, Func<DateTime> todayFn);
    
    Task<decimal> FindOpenInterestChangePercentMaxAsync(string ticker, Func<DateTime> todayFn);

    Task<IEnumerable<OptionChange>> FindTopsAsync(string ticker, int count, Func<DateTime> todayFn);
}