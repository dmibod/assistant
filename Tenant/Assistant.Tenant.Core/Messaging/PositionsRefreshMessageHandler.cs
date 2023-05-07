namespace Assistant.Tenant.Core.Messaging;

using Assistant.Tenant.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Common.Core.Messaging.Models;
using Microsoft.Extensions.Logging;

[Handler("{PositionRefreshTopic}")]
public class PositionsRefreshMessageHandler : IMessageHandler<TenantMessage>
{
    private readonly IPublishingService publishingService;
    private readonly ILogger<PositionsRefreshMessageHandler> logger;

    public PositionsRefreshMessageHandler(IPublishingService publishingService, ILogger<PositionsRefreshMessageHandler> logger)
    {
        this.publishingService = publishingService;
        this.logger = logger;
    }

    public Task HandleAsync(TenantMessage message)
    {
        this.logger.LogInformation("Received positions refresh message for {Tenant}", message.Tenant);
        
        return this.publishingService.PublishPositionsAsync();
    }
}