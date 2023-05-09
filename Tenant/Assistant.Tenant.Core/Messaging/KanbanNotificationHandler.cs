namespace Assistant.Tenant.Core.Messaging;

using System.Text.Json.Serialization;
using Assistant.Tenant.Core.Services;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Microsoft.Extensions.Logging;

[Handler("notification")]
public class KanbanNotificationHandler : IMessageHandler<List<KanbanNotification>>
{
    private readonly IPositionService positionService;
    private readonly ILogger<KanbanNotificationHandler> logger;

    public KanbanNotificationHandler(IPositionService positionService, ILogger<KanbanNotificationHandler> logger)
    {
        this.positionService = positionService;
        this.logger = logger;
    }

    public async Task HandleAsync(List<KanbanNotification> notifications)
    {
        //this.logger.LogInformation("Received kanban notifications {Count}", notifications.Count);

        foreach (var notification in notifications)
        {
            switch (notification.NotificationType)
            {
                case KanbanNotificationType.RemoveCardNotification:
                {
                    await this.HandleRemoveCard(notification);
                } break;
            }
        }
    }

    private async Task HandleRemoveCard(KanbanNotification notification)
    {
        var positionBoardId = await this.positionService.FindPositionsBoardId();

        if (!string.IsNullOrEmpty(positionBoardId) && positionBoardId == notification.BoardId)
        {
            this.LogNotification(notification);

            var positions = await this.positionService.FindByCardIdAsync(notification.EntityId);

            foreach (var position in positions)
            {
                await this.positionService.RemoveAsync(position.Account, position.Ticker, true);
            }
        }
    }

    private void LogNotification(KanbanNotification notification)
    {
        this.logger.LogInformation("Received kanban notification '{Type}' for '{Entity}' of '{Board}'",
            notification.NotificationType,
            notification.EntityId, 
            notification.BoardId);
    }
}

public enum KanbanNotificationType
{
    RefreshCardNotification,
    RefreshLaneNotification,
    RefreshBoardNotification,
    RemoveCardNotification,
    RemoveLaneNotification,
    RemoveBoardNotification,
    CreateCardNotification,
    CreateLaneNotification,
    CreateBoardNotification
}

public class KanbanNotification
{
    [JsonPropertyName("id")]
    public string EntityId { get; set; }
    
    [JsonPropertyName("board_id")]
    public string BoardId { get; set; }

    [JsonPropertyName("type")]
    public KanbanNotificationType NotificationType { get; set; }
}
