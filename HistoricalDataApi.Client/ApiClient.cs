namespace HistoricalDataApi.Client;

using System.Text.Json;

public class ApiClient : IDisposable
{
    private const string ApiKeyEnvVar = "HistoricalDataApiKey";
    private static readonly Uri ApiUri = new("https://eodhistoricaldata.com");
    private readonly string ApiKey;

    private readonly HttpClient httpClient;

    public ApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = ApiUri;

        this.ApiKey = Environment.GetEnvironmentVariable(ApiKeyEnvVar, EnvironmentVariableTarget.Machine);
    }

    public async Task<OptionsResponse?> OptionsAsync(OptionsRequest request)
    {
        var requestUri = $"/api/options/{request.Ticker}.US?api_token={this.ApiKey}";
        
        using var response = await this.httpClient.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<OptionsResponse>(content);
    }
    
    public void Dispose()
    {
        this.httpClient.Dispose();
    }
}