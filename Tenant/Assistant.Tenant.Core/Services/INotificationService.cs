namespace Assistant.Tenant.Core.Services;

public interface INotificationService
{
    Task NotifyRefreshPositionsAsync();
    
    Task NotifyRefreshWatchListAsync();
}