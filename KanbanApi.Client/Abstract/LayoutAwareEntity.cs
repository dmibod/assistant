namespace KanbanApi.Client.Abstract;

using System.Text.Json.Serialization;

public abstract class LayoutAwareEntity : NamedEntity
{
    [JsonPropertyName("layout")]
    public LayoutTypes Layout { get; set; }
}