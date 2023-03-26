namespace KanbanApi.Client;

using System.Text.Json.Serialization;
using KanbanApi.Client.Abstract;

public class Command : Entity
{
    [JsonPropertyName("board_id")]
    public string BoardId { get; set; }

    [JsonPropertyName("type")]
    public int CommandType { get; set; }

    [JsonPropertyName("payload")]
    public IDictionary<string, string> Payload { get; set; } = new Dictionary<string, string>();
}