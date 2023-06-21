namespace PolygonApi.Client;

using System.Text.Json;
using PolygonApi.Client.Utils;

public class ApiClient : IDisposable
{
    private static readonly Uri ApiUri = new("https://api.polygon.io");
    private readonly HttpClient httpClient;

    public ApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = ApiUri;
    }

    public async Task<PrevCloseResponse?> PrevCloseAsync(PrevCloseRequest request)
    {
        var requestUri = $"/v2/aggs/ticker/{request.Ticker}/prev?adjusted=true";

        using var response = await this.httpClient.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<PrevCloseResponse>(content);
    }

    public async Task<PrevCloseResponse?> PrevCloseAsync(PrevCloseOptionRequest request)
    {
        var requestUri =
            $"/v2/aggs/ticker/O:{request.Ticker}{request.Expiration}{request.Side}{Formatting.FormatStrike(request.Strike)}/prev?adjusted=true";

        using var response = await this.httpClient.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<PrevCloseResponse>(content);
    }

    public async Task<OptionChainResponse?> OptionChainAsync(OptionChainRequest request)
    {
        var requestUri = $"/v3/snapshot/options/{request.Ticker}";

        using var response = await this.httpClient.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<OptionChainResponse>(content);
    }

    public async IAsyncEnumerable<OptionChainResponse?> OptionChainStreamAsync(OptionChainRequest request)
    {
        var baseUri = $"/v3/snapshot/options/{request.Ticker}";
        var requestUri = baseUri;

        while (!string.IsNullOrEmpty(requestUri))
        {
            using var response = await this.httpClient.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
            {
                break;
            }

            var chainResponse = JsonSerializer.Deserialize<OptionChainResponse>(content);

            if (chainResponse == null)
            {
                break;
            }

            if (string.IsNullOrEmpty(chainResponse.NextUrl))
            {
                requestUri = null;
            }
            else
            {
                var uri = new Uri(chainResponse.NextUrl);

                requestUri = $"{baseUri}{uri.Query}";
            }

            yield return chainResponse;
        }
    }

    public IEnumerable<OptionChainResponse?> OptionChainStream(OptionChainRequest request)
    {
        var requestUri = $"/v3/snapshot/options/{request.Ticker}";

        while (!string.IsNullOrEmpty(requestUri))
        {
            using var response = this.httpClient.GetAsync(requestUri).Result;

            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsStringAsync().Result;

            if (string.IsNullOrEmpty(content))
            {
                break;
            }

            var chainResponse = JsonSerializer.Deserialize<OptionChainResponse>(content);

            if (chainResponse == null)
            {
                break;
            }

            requestUri = chainResponse.NextUrl;

            yield return chainResponse;
        }
    }

    public void Dispose()
    {
        this.httpClient.Dispose();
    }
}