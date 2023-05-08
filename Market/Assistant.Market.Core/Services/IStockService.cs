namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;

public interface IStockService
{
    Task<Stock> GetOrCreateAsync(string ticker);

    Task UpdateAsync(Stock stock);
    
    Task<string?> FindOutdatedTickerAsync(TimeSpan olderThan);
    
    Task<IEnumerable<Stock>> FindAllAsync();
    
    Task<IEnumerable<string>> FindTickersAsync();
    
    Task<Stock?> FindByTickerAsync(string ticker);
    
    Task RemoveAsync(string ticker);
}