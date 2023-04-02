namespace PolygonApi.Client.Tests;

using PolygonApi.Client;
using PolygonApi.Client.Utils;

[TestClass]
public class ApiClientTests
{
    private static ApiClient client;

    [ClassInitialize]
    public static void TestFixtureSetup(TestContext context)
    {
        var httpClient = new HttpClient();
        
        client = new ApiClient(httpClient);
    }

    [ClassCleanup]
    public static void TestFixtureTearDown()
    {
        client.Dispose();
    }

    [TestMethod]
    public async Task PrevCloseAsync_ReturnsExpectedResult()
    {
        // Arrange
        var request = new PrevCloseRequest
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