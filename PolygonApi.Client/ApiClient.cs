namespace Polygon.Client;

using System.Text.Json;
using Polygon.Client.Utils;

public class ApiClient : IDisposable
{
    private const string DefaultApiKey = "gpoaGtajTlrCLYIFRAqeMK7rAS7QfRdl";
    private const string ApiKeyEnvVar = "PolygonApiKey";
    private static readonly Uri ApiUri = new Uri("https://api.polygon.io");

    private readonly HttpClient httpClient;

    public ApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = ApiUri;

        var apiKey = Environment.GetEnvironmentVariable(ApiKeyEnvVar, EnvironmentVariableTarget.Machine) ?? DefaultApiKey;
        
        this.httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<PrevCloseResponse?> PrevCloseAsync(PrevCloseRequest request)
    {
        var requestUri = $"/v2/aggs/ticker/O:{request.Ticker}{request.Expiration}{request.Side}{Formatting.FormatStrike(request.Strike)}/prev?adjusted=true";
        
        using var response = await this.httpClient.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<PrevCloseResponse>(content);
    }

    public async Task<OptionChainResponse> OptionChainAsync(OptionChainRequest request)
    {
        // https://api.polygon.io/v3/snapshot/options/AAPL?apiKey=gpoaGtajTlrCLYIFRAqeMK7rAS7QfRdl
        var requestUri = $"/v3/snapshot/options/{request.Ticker}";
        
        using var response = await this.httpClient.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<OptionChainResponse>(content);
    }

    public async IAsyncEnumerable<OptionChainResponse?> OptionChainStreamAsync(OptionChainRequest request)
    {
        // https://api.polygon.io/v3/snapshot/options/AAPL?apiKey=gpoaGtajTlrCLYIFRAqeMK7rAS7QfRdl
        var requestUri = $"/v3/snapshot/options/{request.Ticker}";

        do
        {
            using var response = await this.httpClient.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var chainResponse = JsonSerializer.Deserialize<OptionChainResponse>(content);

            if (chainResponse == null)
            {
                break;
            }

            requestUri = chainResponse.NextUrl;
                
            yield return chainResponse;

        } while (!string.IsNullOrEmpty(requestUri));
    }

    public void Dispose()
    {
        this.httpClient.Dispose();
    }
}