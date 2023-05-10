namespace Assistant.Tenant.Core.Repositories;

using Assistant.Tenant.Core.Models;

public interface ITenantRepository
{
    Task<IEnumerable<string>> FindAllTenantsAsync();
    
    Task<Tenant> FindByNameAsync(string name);
    
    Task<bool> ExistsAsync(string name);
    
    Task CreateAsync(string name);

    Task<IEnumerable<Position>> FindPositionsAsync(string tenant);
    
    Task<IEnumerable<Position>> FindPositionsAsync(string tenant, Func<Position, bool> criteria);

    Task<Position?> FindPositionAsync(string tenant, Func<Position, bool> criteria);
    
    Task CreatePositionAsync(string tenant, Position position);

    Task UpdatePositionAsync(string tenant, Position position);

    Task RemovePositionAsync(string tenant, string account, string ticker);
    
    Task TagPositionAsync(string tenant, string account, string ticker, string tag);
    
    Task KanbanPositionAsync(string tenant, string account, string ticker, string cardId);
    
    Task<IEnumerable<WatchListItem>> FindWatchListAsync(string tenant);
    
    Task<IEnumerable<WatchListItem>> FindWatchListAsync(string tenant, Func<WatchListItem, bool> criteria);

    Task CreateWatchListItemAsync(string tenant, WatchListItem listItem);
    
    Task RemoveWatchListItemAsync(string tenant, string ticker);

    Task SetWatchListItemBuyPriceAsync(string tenant, string ticker, decimal price);
    
    Task SetWatchListItemSellPriceAsync(string tenant, string ticker, decimal price);
    
    Task SetWatchListItemPricesAsync(string tenant, string ticker, decimal buyPrice, decimal sellPrice);
    
    Task ResetTagAsync(string tenant);
    
    Task ReplaceTagAsync(string tenant, string oldValue, string newValue);
    
    Task<string?> FindSellPutsFilterAsync(string tenant);

    Task UpdateSellPutsFilterAsync(string tenant, string filter);

    Task<string?> FindSellCallsFilterAsync(string tenant);

    Task UpdateSellCallsFilterAsync(string tenant, string filter);

    Task<string?> FindPositionsBoardIdAsync(string tenant);

    Task UpdatePositionsBoardIdAsync(string tenant, string boardId);
    
    Task<string?> FindSellPutsBoardIdAsync(string tenant);
    
    Task UpdateSellPutsBoardIdAsync(string tenant, string boardId);
    
    Task<string?> FindSellCallsBoardIdAsync(string tenant);
    
    Task UpdateSellCallsBoardIdAsync(string tenant, string boardId);
    
    Task<string?> FindOpenInterestFilterAsync(string tenant);

    Task UpdateOpenInterestFilterAsync(string tenant, string filter);
    
    Task<string?> FindOpenInterestBoardIdAsync(string tenant);
    
    Task UpdateOpenInterestBoardIdAsync(string tenant, string boardId);
    
    Task UpdateWatchListBoardIdAsync(string tenant, string boardId);
    
    Task<string?> FindWatchListBoardIdAsync(string tenant);
    
    Task KanbanWatchListAsync(string tenant, string ticker, string cardId);
}