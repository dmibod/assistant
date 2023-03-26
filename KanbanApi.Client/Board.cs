namespace KanbanApi.Client;

using System.Text.Json.Serialization;
using KanbanApi.Client.Abstract;

public class Board : LayoutAwareEntity
{
    [JsonPropertyName("owner")]
    public string Owner { get; set; }

    [JsonPropertyName("shared")]
    public bool Shared { get; set; }
}