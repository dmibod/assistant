namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface IPositionService
{
    Task<IEnumerable<Position>> FindAllAsync();

    Task<IEnumerable<Position>> FindByCardIdAsync(string cardId);

    Task<Position> CreateAsync(Position position);

    Task<Position> CreateOrUpdateAsync(Position position);
    
    Task UpdateAsync(string account, string ticker, int quantity, decimal averageCost);

    Task UpdateTagAsync(string account, string ticker, string tag);
    
    Task UpdateCardIdAsync(string account, string ticker, string cardId);

    Task RemoveAsync(string account, string ticker, bool suppressNotifications);

    Task ResetTagAsync();

    Task ReplaceTagAsync(string oldValue, string newValue);

    Task AutoTagOptionsAsync();

    Task<string?> FindPositionsBoardId();

    Task UpdatePositionsBoardId(string positionsBoardId);
}