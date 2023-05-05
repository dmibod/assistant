namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface IPositionService
{
    Task<IEnumerable<Position>> FindAllAsync();
    
    Task<Position> CreateAsync(Position position);

    Task<Position> CreateOrUpdateAsync(Position position);
    
    Task UpdateAsync(string account, string ticker, int quantity, decimal averageCost);

    Task UpdateTagAsync(string account, string ticker, string tag);

    Task RemoveAsync(string account, string ticker);

    Task ResetTagAsync();

    Task ReplaceTagAsync(string oldValue, string newValue);

    Task AutoTagOptionsAsync();
}