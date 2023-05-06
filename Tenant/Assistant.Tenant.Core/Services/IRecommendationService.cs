namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Common.Core.Utils;
using Helper.Core.Domain;

public interface IRecommendationService
{
    Task<IEnumerable<SellOperation>> SellPutsAsync(RecommendationFilter filter, Func<int, ProgressTracker> trackerCreator);
    
    Task<IEnumerable<SellOperation>> SellCallsAsync(RecommendationFilter filter, bool considerPositions, Func<int, ProgressTracker> trackerCreator);
}