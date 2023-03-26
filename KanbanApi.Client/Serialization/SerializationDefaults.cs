namespace KanbanApi.Client.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class SerializationDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
}