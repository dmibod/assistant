namespace Assistant.Market.Tests;

using Assistant.Market.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PolygonApi.Client;

[TestClass]
public class MarketDataServiceTests
{
    private const string Token = "";

    [TestMethod]
    public async Task GetStockPriceAsync_ReturnsExpectedResult()
    {
        // Arrange
        var httpClient = new HttpClient(new HttpClientHandler());
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token}");
        var service = new MarketDataService(new HttpClientFactory(httpClient), Substitute.For<ILogger<MarketDataService>>());
        
        // Act
        var stockPrice = await service.GetStockPriceAsync("FSR");
        
        // Assert
        Assert.IsTrue(stockPrice.TimeStamp < DateTime.Now);
    }

    [TestMethod]
    public async Task GetOptionChainAsync_ReturnsExpectedResult()
    {
        // Arrange
        var httpClient = new HttpClient(new HttpClientHandler());
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token}");
        var service = new MarketDataService(new HttpClientFactory(httpClient), Substitute.For<ILogger<MarketDataService>>());
        
        // Act
        var optionChain = await service.GetOptionChainAsync("PYPL");
        
        // Assert
        var put = optionChain.Expirations["20250117"].Contracts[5].Put;
        Assert.IsTrue(put.Last < 0.9m);
    }
}

internal class HttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient httpClient;

    public HttpClientFactory(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public HttpClient CreateClient(string name)
    {
        return this.httpClient;
    }
}