namespace KanbanApi.Client.Abstract;

using System.Text.Json.Serialization;

public abstract class Entity
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}