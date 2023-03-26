namespace KanbanApi.Client;

using System.Text.Json.Serialization;

public class CommandResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}