namespace Assistant.Tenant.Core.Messaging;

using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Common.Core.Messaging.Models;
using Microsoft.Extensions.Logging;

[Handler("{ScheduleTopic}")]
public class ScheduleMessageHandler : IMessageHandler<ScheduleMessage>
{
    private readonly IScheduleService scheduleService;
    private readonly IPositionPublishingService positionPublishingService;
    private readonly IWatchListPublishingService watchListPublishingService;
    private readonly IRecommendationPublishingService recommendationPublishingService;
    private readonly ILogger<ScheduleMessageHandler> logger;

    public ScheduleMessageHandler(
        IScheduleService scheduleService, 
        IPositionPublishingService positionPublishingService,
        IWatchListPublishingService watchListPublishingService,
        IRecommendationPublishingService recommendationPublishingService,
        ILogger<ScheduleMessageHandler> logger)
    {
        this.scheduleService = scheduleService;
        this.positionPublishingService = positionPublishingService;
        this.watchListPublishingService = watchListPublishingService;
        this.recommendationPublishingService = recommendationPublishingService;
        this.logger = logger;
    }

    public async Task HandleAsync(ScheduleMessage message)
    {
        this.logger.LogInformation("Received schedule message for {Tenant}", message.Tenant);

        var schedules = await this.scheduleService.FindAllAsync();

        foreach (var schedule in schedules.Where(s => s.Interval != ScheduleInterval.None))
        {
            var diff = DateTime.UtcNow - schedule.LastExecution;
            var threshold = schedule.Interval == ScheduleInterval.Hourly ? TimeSpan.FromHours(1) : TimeSpan.FromDays(1);
            
            if (diff > threshold)
            {
                await this.scheduleService.ExecuteScheduleAsync(schedule.ScheduleType);
                
                switch (schedule.ScheduleType)
                {
                    case ScheduleType.Positions:
                        await this.positionPublishingService.PublishAsync();
                        break;
                    case ScheduleType.WatchList:
                        await this.watchListPublishingService.PublishAsync();
                        break;
                    case ScheduleType.SellPuts:
                        await this.recommendationPublishingService.PublishSellPutsAsync();
                        break;
                    case ScheduleType.SellCalls:
                        await this.recommendationPublishingService.PublishSellCallsAsync();
                        break;
                    case ScheduleType.OpenInterest:
                        await this.recommendationPublishingService.PublishOpenInterestAsync();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}

public class ScheduleMessage : TenantMessage
{
}