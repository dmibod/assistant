namespace KanbanApi.Client;

using System.Text.Json.Serialization;
using KanbanApi.Client.Abstract;

public class Board : NamedEntity
{
    [JsonPropertyName("owner")]
    public string Owner { get; set; }

    [JsonPropertyName("layout")]
    public LayoutTypes Layout { get; set; }

    [JsonPropertyName("shared")]
    public bool Shared { get; set; }
}