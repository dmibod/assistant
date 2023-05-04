namespace Assistant.Tenant.Infrastructure.Services;

using Assistant.Tenant.Core.Services;
using Assistant.Tenant.Infrastructure.Configuration;
using Common.Core.Security;
using Common.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class NotificationService : INotificationService
{
    private readonly IIdentityProvider identityProvider;
    private readonly IBusService busService;
    private readonly ILogger<NotificationService> logger;
    private readonly string refreshPositionTopic;

    public NotificationService(IIdentityProvider identityProvider, IOptions<NatsSettings> options, IBusService busService, ILogger<NotificationService> logger)
    {
        this.refreshPositionTopic = options.Value.RefreshTenantPositionTopic;
        this.identityProvider = identityProvider;
        this.busService = busService;
        this.logger = logger;
    }

    public Task NotifyRefreshPositionsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.NotifyRefreshPositionsAsync));

        return this.busService.PublishAsync(this.refreshPositionTopic, this.identityProvider.Identity.Name);
    }
}