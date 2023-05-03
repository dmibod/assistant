namespace Assistant.Market.Core.Services;

public interface IRefreshService
{
    Task CleanAsync(DateTime now);

    Task UpdateStockAsync(string ticker);
}