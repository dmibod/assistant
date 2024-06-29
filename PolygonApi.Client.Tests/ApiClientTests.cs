namespace PolygonApi.Client.Tests;

using PolygonApi.Client;
using PolygonApi.Client.Utils;

[TestClass]
public class ApiClientTests
{
    private const string Token = "";
    private static ApiClient client;

    [ClassInitialize]
    public static void TestFixtureSetup(TestContext context)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token}");
        client = new ApiClient(httpClient);
    }

    [ClassCleanup]
    public static void TestFixtureTearDown()
    {
        client.Dispose();
    }

    [TestMethod]
    public async Task TickerDetailsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var request = new TickerDetailsRequest
        {
            Ticker = "AAPL"
        };
        
        // Act
        var response = await client.TickerDetailsAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Results);
        Assert.IsTrue(response.Results.MarketCap > 0);
    }

    [TestMethod]
    public async Task PrevCloseAsync_ReturnsExpectedResult()
    {
        // Arrange
        var request = new PrevCloseOptionRequest
        {
            Ticker = "SPY",
            Expiration = Formatting.ToExpiration(Formatting.GetNextWeekday(DayOfWeek.Wednesday)),
            Side = "P",
            Strike = 400
        };
        
        // Act
        var response = await client.PrevCloseAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Results);
        Assert.IsTrue(response.Results.Length > 0);
    }
    
    [TestMethod]
    public async Task OptionChainAsync_ReturnsExpectedResult()
    {
        // Arrange
        var request = new OptionChainRequest
        {
            Ticker = "ZIM"
        };
        
        // Act
        var response = await client.OptionChainAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Results);
        Assert.IsTrue(response.Results.Length > 0);
    }
    
    [TestMethod]
    public async Task OptionChainStream_ReturnsExpectedResult()
    {
        // Arrange
        var request = new OptionChainRequest
        {
            Ticker = "FSR"
        };
        
        // Act
        var optionChain = client
            .OptionChainStream(request)
            .SelectMany(item => item.Results)
            .Where(item => item.Day.Close > decimal.Zero)
            .GroupBy(item => item.Details.ExpirationDate)
            .ToDictionary(item => item.Key.Replace("-", string.Empty), item => item.ToList());

        // Assert
        Assert.IsTrue(optionChain.Count > 0);
    }

    [TestMethod]
    public async Task OptionChainStreamAsync_ReturnsExpectedResult()
    {
        // Arrange
        var request = new OptionChainRequest
        {
            Ticker = "ZIM"
        };
        
        // Act
        var responses = new List<OptionChainResponse>();
        
        await foreach (var response in client.OptionChainStreamAsync(request))
        {
            responses.Add(response);
        }

        // Assert
        Assert.IsTrue(responses.Count > 0);
    }
}