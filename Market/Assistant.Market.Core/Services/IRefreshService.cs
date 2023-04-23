namespace Assistant.Market.Core.Services;

public interface IRefreshService
{
    Task RefreshAsync(TimeSpan lag);
}