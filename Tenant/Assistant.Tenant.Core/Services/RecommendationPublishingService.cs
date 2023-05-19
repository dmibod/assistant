namespace Assistant.Tenant.Core.Services;

using Microsoft.Extensions.Logging;

public class RecommendationPublishingService : IRecommendationPublishingService
{
    private readonly IRecommendationService recommendationService;
    private readonly IPublishingService publishingService;
    private readonly ILogger<RecommendationPublishingService> logger;

    public RecommendationPublishingService(
        IRecommendationService recommendationService, 
        IPublishingService publishingService, 
        ILogger<RecommendationPublishingService> logger)
    {
        this.recommendationService = recommendationService;
        this.publishingService = publishingService;
        this.logger = logger;
    }

    public async Task PublishSellPutsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishSellPutsAsync));
        
        var filter = await this.recommendationService.GetSellPutsFilterAsync();
        
        await this.publishingService.PublishSellPutsAsync(filter);
    }

    public async Task PublishSellCallsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishSellCallsAsync));
        
        var filter = await this.recommendationService.GetSellCallsFilterAsync();
        
        await this.publishingService.PublishSellCallsAsync(filter);
    }

    public async Task PublishOpenInterestAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishOpenInterestAsync));

        var filter = await this.recommendationService.GetOpenInterestFilterAsync();
        
        await this.publishingService.PublishOpenInterestAsync(filter);
    }
}