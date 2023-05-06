namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface IPublishingService
{
    Task PublishSellPutsAsync(RecommendationFilter filter);
    
    Task PublishSellCallsAsync(RecommendationFilter filter, bool considerPositions);
}