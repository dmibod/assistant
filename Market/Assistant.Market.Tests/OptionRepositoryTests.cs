namespace Assistant.Market.Tests;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Repositories;
using Assistant.Market.Infrastructure.Repositories;
using Helper.Core.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

[TestClass]
public class OptionRepositoryTests
{
    private const string Ticker = "FSR";
    private const string Expiration = "20230519";
    
    private static IOptionRepository Repository =>
        new OptionRepository(
            "mongodb://localhost:27017",
            "assistant", 
            "option", 
            Substitute.For<ILogger<OptionRepository>>());

    [TestMethod]
    public async Task ExistsAsync_ReturnsExpectedResult()
    {
        // Arrange & Act
        var result = await Repository.ExistsAsync(Ticker, Expiration);
        
        // Assert
        Assert.IsTrue(result);
    }
    
    [TestMethod]
    public async Task FindByTickerAsync_ReturnsExpectedResult()
    {
        // Arrange & Act
        var result = await Repository.FindByTickerAsync(Ticker);
        
        // Assert
        var list = result.ToList();
        Assert.IsTrue(list.Count > 0);
    }
    
    [TestMethod]
    public async Task FindExpirationsAsync_ReturnsExpectedResult()
    {
        // Arrange & Act
        var result = await Repository.FindExpirationsAsync(Ticker);
        
        // Assert
        var list = result.ToList();
        Assert.IsTrue(list.Count > 0);
    }

    [TestMethod]
    public async Task CreateAsync_ReturnsExpectedResult()
    {
        // Arrange & Act
        const string ticker = "123";

        var repository = Repository;
        
        await repository.CreateAsync(new Option
        {
            Ticker = ticker,
            Expiration = Expiration,
            Contracts = Array.Empty<OptionContract>()
        });
        
        // Assert
        var exists = await repository.ExistsAsync(ticker, Expiration);
        
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsExpectedResult()
    {
        // Arrange & Act
        const string ticker = "123";

        var repository = Repository;
        
        await repository.UpdateAsync(new Option
        {
            Ticker = ticker,
            Expiration = Expiration,
            Contracts = new []
            {
                new OptionContract
                {
                    Ticker = OptionUtils.OptionTicker(ticker, Expiration, 5, true),
                    Ask = 3,
                    Bid = 1,
                    Last = 2
                }
            }
        });
        
        // Assert
        var options = await repository.FindByTickerAsync(ticker);

        var option = options.First();
        
        Assert.IsTrue(option.Contracts.Length == 1);
    }

    [TestMethod]
    public async Task RemoveAsync_ReturnsExpectedResult()
    {
        // Arrange & Act
        const string ticker = "123";

        var repository = Repository;
        
        await repository.RemoveAsync(new Dictionary<string, ISet<string>>
        {
            [ ticker ] = new HashSet<string> { Expiration }
        });
        
        // Assert
        var exists = await repository.ExistsAsync(ticker, Expiration);
        
        Assert.IsFalse(exists);
    }
}