namespace Assistant.Tenant.Core.Messaging;

using Assistant.Tenant.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Microsoft.Extensions.Logging;

[Handler("{PositionCreateTopic}")]
public class PositionCreateMessageHandler : IMessageHandler<PositionCreateMessage>
{
    private readonly IPositionService positionService;
    private readonly ILogger<PositionCreateMessageHandler> logger;

    public PositionCreateMessageHandler(IPositionService positionService, ILogger<PositionCreateMessageHandler> logger)
    {
        this.positionService = positionService;
        this.logger = logger;
    }

    public Task HandleAsync(PositionCreateMessage message)
    {
        this.logger.LogInformation("Received message to add position: {Account} {Ticker} for {Tenant}", message.Account, message.Ticker, message.Tenant);
        
        return this.positionService.CreateOrUpdateAsync(message.AsPosition());
    }
}