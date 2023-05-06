namespace Assistant.Tenant.Core.Messaging;

using System.Text.Json.Serialization;

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
