namespace Assistant.Tenant.Infrastructure.Services;

using Assistant.Tenant.Core.Messaging;
using Assistant.Tenant.Core.Services;
using Assistant.Tenant.Infrastructure.Configuration;
using Common.Core.Messaging.TopicResolver;
using Common.Core.Security;
using Common.Core.Services;
using Microsoft.Extensions.Logging;

public class NotificationService : INotificationService
{
    private readonly IIdentityProvider identityProvider;
    private readonly IBusService busService;
    private readonly ILogger<NotificationService> logger;
    private readonly string refreshPositionTopic;
    private readonly string refreshWatchListTopic;

    public NotificationService(
        IIdentityProvider identityProvider,
        ITopicResolver topicResolver,
        IBusService busService,
        ILogger<NotificationService> logger)
    {
        this.refreshPositionTopic = topicResolver.ResolveConfig(nameof(NatsSettings.PositionRefreshTopic));
        this.refreshWatchListTopic = topicResolver.ResolveConfig(nameof(NatsSettings.WatchListRefreshTopic));
        this.identityProvider = identityProvider;
        this.busService = busService;
        this.logger = logger;
    }

    public Task NotifyRefreshPositionsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.NotifyRefreshPositionsAsync));

        return this.busService.PublishAsync(this.refreshPositionTopic, new PositionRefreshMessage
        {
            Tenant = this.identityProvider.Identity.Name!
        });
    }

    public Task NotifyRefreshWatchListAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.NotifyRefreshWatchListAsync));

        return this.busService.PublishAsync(this.refreshWatchListTopic, new WatchListRefreshMessage
        {
            Tenant = this.identityProvider.Identity.Name!
        });
    }
}