namespace Assistant.Tenant.Core.Messaging;

using Assistant.Tenant.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Common.Core.Messaging.Models;
using Microsoft.Extensions.Logging;

[Handler("{PositionRefreshTopic}")]
public class PositionRefreshMessageHandler : IMessageHandler<PositionRefreshMessage>
{
    private readonly IPublishingService publishingService;
    private readonly ILogger<PositionRefreshMessageHandler> logger;

    public PositionRefreshMessageHandler(IPublishingService publishingService, ILogger<PositionRefreshMessageHandler> logger)
    {
        this.publishingService = publishingService;
        this.logger = logger;
    }

    public Task HandleAsync(PositionRefreshMessage message)
    {
        this.logger.LogInformation("Received positions refresh message for {Tenant}", message.Tenant);
        
        return this.publishingService.PublishPositionsAsync();
    }
}

public class PositionRefreshMessage : TenantMessage
{
}