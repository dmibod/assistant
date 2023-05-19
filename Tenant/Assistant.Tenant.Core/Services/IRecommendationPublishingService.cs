namespace Assistant.Tenant.Core.Services;

public interface IRecommendationPublishingService
{
    Task PublishSellPutsAsync();
    
    Task PublishSellCallsAsync();
    
    Task PublishOpenInterestAsync();

}