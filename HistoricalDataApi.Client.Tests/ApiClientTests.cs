namespace HistoricalDataApi.Client.Tests;

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
    public async Task OptionsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var request = new OptionsRequest
        {
            Ticker = "ALLY"
        };
        
        // Act
        var response = await client.OptionsAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Expirations);
        Assert.IsTrue(response.Expirations.Length > 0);
    }
}