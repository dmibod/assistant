namespace Assistant.Tenant.Core.Messaging;

using System.Text.Json.Serialization;

public enum KanbanCommandType
{
    UpdateCardCommand,
    RemoveCardCommand,
    UpdateLaneCommand,
    RemoveLaneCommand,
    ExcludeChildCommand,
    AppendChildCommand,
    InsertBeforeCommand,
    InsertAfterCommand,
    LayoutBoardCommand,
    LayoutLaneCommand,
    DescribeBoardCommand,
    DescribeLaneCommand,
    DescribeCardCommand,
    UpdateBoardCommand,
    StateBoardCommand
}

public class KanbanCommand
{
    [JsonPropertyName("id")]
    public string EntityId { get; set; }
    
    [JsonPropertyName("board_id")]
    public string BoardId { get; set; }

    [JsonPropertyName("type")]
    public KanbanCommandType CommandType { get; set; }

    [JsonPropertyName("payload")]
    public IDictionary<string, string> Payload { get; set; }
}