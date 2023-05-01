namespace Assistant.Market.Tests;

using Assistant.Market.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PolygonApi.Client;

[TestClass]
public class MarketDataServiceTests
{
    [TestMethod]
    public async Task GetOptionChainAsync_ReturnsExpectedResult()
    {
        // Arrange
        var httpClient = new HttpClient(new HttpClientHandler());
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer {token}");
        var client = new ApiClient(httpClient);
        var service = new MarketDataService(client, Substitute.For<ILogger<MarketDataService>>());
        
        // Act
        var optionChain = await service.GetOptionChainAsync("FSR");
        
        // Assert
        var put = optionChain.Expirations["20230519"].Contracts[5].Put;
        Assert.IsTrue(put.Last < 0.9m);
    }
}