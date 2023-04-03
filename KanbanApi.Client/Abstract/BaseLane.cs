namespace KanbanApi.Client.Abstract;

using System.Text.Json.Serialization;

public abstract class BaseLane : NamedEntity
{
    [JsonPropertyName("type")]
    public LaneTypes Type { get; set; }
}