namespace Assistant.Market.Core.Repositories;

using Assistant.Market.Core.Models;

public interface IStockRepository
{
    Task<bool> ExistsAsync(string ticker);
    
    Task<Stock?> FindByTickerAsync(string ticker);
    
    Task CreateAsync(Stock stock);

    Task UpdateAsync(Stock stock);

    Task<Stock?> FindOldestAsync(TimeSpan olderThan);
    
    Task<IEnumerable<Stock>> FindAllAsync();
}