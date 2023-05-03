namespace Assistant.Market.Tests;

using Assistant.Market.Core.Repositories;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;

[TestClass]
public class RefreshServiceTests
{
    private static IStockRepository StockRepository =>
        new StockRepository(
            "mongodb://localhost:27017",
            "assistant", 
            "stock", 
            Substitute.For<ILogger<StockRepository>>());

    private static IOptionRepository OptionRepository =>
        new OptionRepository(
            "mongodb://localhost:27017",
            "assistant", 
            "option", 
            Substitute.For<ILogger<OptionRepository>>());

    private static IOptionChangeRepository OptionChangeRepository =>
        new OptionChangeRepository(
            "mongodb://localhost:27017",
            "assistant", 
            "option-change", 
            Substitute.For<ILogger<OptionChangeRepository>>());

    [TestMethod]
    public async Task GetOptionChainAsync_ReturnsExpectedResult()
    {
        // Arrange
        var stockService = new StockService(StockRepository, Substitute.For<ILogger<StockService>>());
        var optionService = new OptionService(OptionRepository,  OptionChangeRepository, Substitute.For<ILogger<OptionService>>());
        var service = new RefreshService(Substitute.For<IMarketDataService>(), stockService, optionService, Substitute.For<ILogger<RefreshService>>());
        
        // Act
        await service.CleanAsync(DateTime.UtcNow);
        
        // Assert
        Assert.IsTrue(true);
    }
}