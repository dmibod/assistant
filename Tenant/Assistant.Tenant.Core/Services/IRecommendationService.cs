namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Common.Core.Utils;
using Helper.Core.Domain;

public interface IRecommendationService
{
    Task<string?> FindSellPutsBoardId();

    Task UpdateSellPutsBoardId(string boardId);

    Task<SellPutsFilter?> GetSellPutsFilterAsync();

    Task UpdateSellPutsFilterAsync(SellPutsFilter filter);

    Task<IEnumerable<SellOperation>> SellPutsAsync(SellPutsFilter filter, Func<int, ProgressTracker> trackerCreator);
    
    Task<string?> FindSellCallsBoardId();

    Task UpdateSellCallsBoardId(string boardId);

    Task<SellCallsFilter?> GetSellCallsFilterAsync();

    Task UpdateSellCallsFilterAsync(SellCallsFilter filter);

    Task<IEnumerable<SellOperation>> SellCallsAsync(SellCallsFilter filter, Func<int, ProgressTracker> trackerCreator);
    
    Task<string?> FindOpenInterestBoardId();

    Task UpdateOpenInterestBoardId(string boardId);

    Task<OpenInterestFilter?> GetOpenInterestFilterAsync();

    Task UpdateOpenInterestFilterAsync(OpenInterestFilter filter);

    Task<IEnumerable<OptionAssetPrice>> OpenInterestAsync(OpenInterestFilter filter, Func<int, ProgressTracker> trackerCreator);
}