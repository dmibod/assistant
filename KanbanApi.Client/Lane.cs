namespace KanbanApi.Client;

using System.Text.Json.Serialization;
using KanbanApi.Client.Abstract;

public class Lane : BaseLane
{
    [JsonPropertyName("layout")]
    public LayoutTypes Layout { get; set; }
}