namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;

public interface IStockService
{
    Task<Stock?> FindOutdatedWithLagAsync(TimeSpan lag);
}