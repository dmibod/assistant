namespace KanbanApi.Client.Abstract;

using System.Text.Json.Serialization;

public class NamedEntity : Entity
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}