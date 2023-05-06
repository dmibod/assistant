namespace Assistant.Tenant.Core.Messaging;

using Common.Core.Messaging;
using Microsoft.Extensions.Logging;

[Handler("notification", enabled: false)]
public class KanbanNotificationHandler : IMessageHandler<List<KanbanNotification>>
{
    private readonly ILogger<KanbanNotificationHandler> logger;

    public KanbanNotificationHandler(ILogger<KanbanNotificationHandler> logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(List<KanbanNotification> notifications)
    {
        this.logger.LogInformation("Received kanban notifications {Count}", notifications.Count);

        foreach (var notification in notifications)
        {
            this.logger.LogInformation("Received kanban notification '{Type}' for '{Entity}' of '{Board}'",
                notification.NotificationType,
                notification.EntityId, 
                notification.BoardId);
        }
        
        return Task.CompletedTask;
    }
}