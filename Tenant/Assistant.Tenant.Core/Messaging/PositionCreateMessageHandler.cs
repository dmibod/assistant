namespace Assistant.Tenant.Core.Messaging;

using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Common.Core.Messaging.Models;
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

    public async Task HandleAsync(PositionCreateMessage message)
    {
        this.logger.LogInformation("Received message to add position: {Account} {Ticker} for {Tenant}", message.Account, message.Ticker, message.Tenant);

        var position = message.AsPosition();

        if (position.Quantity != 0)
        {
            await this.positionService.CreateOrUpdateAsync(position);
        }
    }
}

public class PositionCreateMessage : Position, ITenantAware
{
    public string Tenant { get; set; }
    
    public Position AsPosition()
    {
        return new Position
        {
            Account = this.Account,
            Quantity = this.Quantity,
            Ticker = this.Ticker,
            Tag = this.Tag,
            Type = this.Type,
            AverageCost = this.AverageCost
        };
    }
}