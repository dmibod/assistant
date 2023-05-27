namespace Assistant.Market.Core.Repositories;

using Assistant.Market.Core.Models;

public interface IOptionChangeRepository
{
    Task<IEnumerable<Option>> FindByTickerAsync(string ticker);

    Task CreateOrUpdateAsync(Option option);

    Task RemoveAsync(IDictionary<string, ISet<string>> expirations);
    
    Task RemoveAsync(string ticker);
    
    Task<int> FindChangesCountAsync(string ticker, Func<DateTime> todayFn);
    
    Task<decimal> FindOpenInterestMinAsync(string ticker, Func<DateTime> todayFn);
    
    Task<decimal> FindOpenInterestMaxAsync(string ticker, Func<DateTime> todayFn);
    
    Task<decimal> FindOpenInterestPercentMinAsync(string ticker, Func<DateTime> todayFn);
    
    Task<decimal> FindOpenInterestPercentMaxAsync(string ticker, Func<DateTime> todayFn);

    Task<IEnumerable<OptionChange>> FindTopsAsync(string ticker, int count, Func<DateTime> todayFn);
}