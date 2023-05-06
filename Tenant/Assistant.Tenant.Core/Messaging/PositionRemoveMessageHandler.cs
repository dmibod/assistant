namespace Assistant.Tenant.Core.Messaging;

using Assistant.Tenant.Core.Services;
using Common.Core.Messaging;
using Microsoft.Extensions.Logging;

[Handler("{PositionRemoveTopic}")]
public class PositionRemoveMessageHandler : IMessageHandler<PositionRemoveMessage>
{
    private readonly IPositionService positionService;
    private readonly ILogger<PositionRemoveMessageHandler> logger;

    public PositionRemoveMessageHandler(
        IPositionService positionService,
        ILogger<PositionRemoveMessageHandler> logger)
    {
        this.positionService = positionService;
        this.logger = logger;
    }

    public async Task HandleAsync(PositionRemoveMessage message)
    {
        this.logger.LogInformation("Received message to remove position: {Account} {Ticker} for {Tenant}", message.Account, message.Ticker, message.Tenant);

        var positions= await this.positionService.FindAllAsync();

        var position = positions.FirstOrDefault(p => p.Account == message.Account && p.Ticker == message.Ticker);
        
        this.logger.LogInformation(position == null ? "Position is not found" : $"Position is found, size is '{position.Quantity}', cost is '{position.AverageCost}'");
    }
}