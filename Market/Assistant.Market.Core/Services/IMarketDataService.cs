namespace Assistant.Market.Core.Services;

using Assistant.Market.Core.Models;

public interface IMarketDataService
{
    Task<AssetPrice?> GetStockPriceAsync(string ticker);
    
    Task<OptionChain?> GetOptionChainAsync(string ticker);
}
