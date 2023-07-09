namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface IWatchListService
{
    Task<IEnumerable<WatchListItem>> FindAllAsync();

    Task<WatchListItem?> FindByTickerAsync(string ticker);

    Task<WatchListItem> CreateOrUpdateAsync(WatchListItem listItem, bool ignoreIfExists, bool suppressNotifications);

    Task RemoveAsync(string ticker, bool suppressNotifications);

    Task SetBuyPriceAsync(string ticker, decimal price);

    Task SetSellPriceAsync(string ticker, decimal price);

    Task SetPricesAsync(string ticker, decimal buyPrice, decimal sellPrice);
    
    Task UpdateKanbanBoardId(string boardId);
    
    Task<string?> FindKanbanBoardId();
    
    Task UpdateKanbanCardIdAsync(string ticker, string cardId);
    
    Task<IEnumerable<WatchListItem>> FindByCardIdAsync(string cardId);
}