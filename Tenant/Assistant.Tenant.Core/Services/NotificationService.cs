namespace Assistant.Tenant.Core.Services;

using Common.Core.Messaging;
using Common.Core.Security;
using Common.Core.Services;
using Microsoft.Extensions.Logging;

public class NotificationService : INotificationService
{
    private readonly IIdentityProvider identityProvider;
    private readonly IBusService busService;
    private readonly ILogger<NotificationService> logger;
    private readonly string refreshPositionTopic;

    public NotificationService(
        IIdentityProvider identityProvider, 
        ITopicResolver topicResolver, 
        IBusService busService,
        ILogger<NotificationService> logger)
    {
        this.refreshPositionTopic = topicResolver.Resolve("{PositionRefreshTopic}");
        this.identityProvider = identityProvider;
        this.busService = busService;
        this.logger = logger;
    }

    public Task NotifyRefreshPositionsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.NotifyRefreshPositionsAsync));

        return this.busService.PublishAsync(this.refreshPositionTopic, new TenantMessage
        {
            Tenant = this.identityProvider.Identity.Name
        });
    }
}