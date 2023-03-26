namespace KanbanApi.Client;

using System.Text.Json.Serialization;
using KanbanApi.Client.Abstract;

public class Lane : LayoutAwareEntity
{
    [JsonPropertyName("type")]
    public LaneTypes Type { get; set; }
}