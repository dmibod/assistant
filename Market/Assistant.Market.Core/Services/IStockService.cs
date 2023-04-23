namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;

public interface IStockService
{
    Task<Stock> GetOrCreateAsync(string ticker);

    Task UpdateAsync(Stock stock);
    
    Task<Stock?> FindOldestAsync(TimeSpan olderThan);
    
    Task<IEnumerable<Stock>> FindAllAsync();
    
    Task<IEnumerable<string>> FindTickersAsync();
}