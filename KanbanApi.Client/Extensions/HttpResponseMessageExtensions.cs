namespace KanbanApi.Client.Extensions;

using System.Text.Json;

public static class HttpResponseMessageExtensions
{
    public static async Task<T?> AsJsonAsync<T>(this HttpResponseMessage response, JsonSerializerOptions options)
    {
        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(content, options);
    }
}