namespace Assistant.Tenant.Core.Messaging;

using Assistant.Tenant.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Common.Core.Messaging.Models;
using Microsoft.Extensions.Logging;

[Handler("{WatchListRefreshTopic}")]
public class WatchListRefreshMessageHandler : IMessageHandler<WatchListRefreshMessage>
{
    private readonly IWatchListPublishingService publishingService;
    private readonly ILogger<WatchListRefreshMessageHandler> logger;

    public WatchListRefreshMessageHandler(IWatchListPublishingService publishingService, ILogger<WatchListRefreshMessageHandler> logger)
    {
        this.publishingService = publishingService;
        this.logger = logger;
    }

    public Task HandleAsync(WatchListRefreshMessage message)
    {
        this.logger.LogInformation("Received watch list refresh message for {Tenant}", message.Tenant);
        
        return this.publishingService.PublishAsync();
    }
}

public class WatchListRefreshMessage : TenantMessage
{
}