namespace Assistant.Market.Tests;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Repositories;
using Assistant.Market.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;

[TestClass]
public class OptionChangeRepositoryTests
{
    private const string Ticker = "123";
    private const string Expiration = "20230519";

    private static IOptionChangeRepository Repository =>
        new OptionChangeRepository(
            "mongodb://localhost:27017",
            "assistant",
            "option-change",
            Substitute.For<ILogger<OptionChangeRepository>>());

    [TestMethod]
    public async Task CreateOrUpdateAsync_PerformsAsExpected()
    {
        // Arrange
        var repository = Repository;

        // Act
        await repository.CreateOrUpdateAsync(new Option
        {
            Ticker = Ticker,
            Expiration = Expiration,
            Contracts = Array.Empty<OptionContract>()
        });

        // Assert
        var expirations = (await repository.FindByTickerAsync(Ticker)).ToArray();

        Assert.AreEqual(1, expirations.Length);
        Assert.AreEqual(0, expirations[0].Contracts.Length);

        // Act
        await repository.CreateOrUpdateAsync(new Option
        {
            Ticker = Ticker,
            Expiration = Expiration,
            Contracts = new[]
            {
                new OptionContract
                {
                    Ticker = $"{Ticker}{Expiration}",
                    TimeStamp = DateTime.UtcNow,
                    OI = 101
                }
            }
        });

        // Assert
        expirations = (await repository.FindByTickerAsync(Ticker)).ToArray();

        Assert.AreEqual(1, expirations.Length);
        Assert.AreEqual(1, expirations[0].Contracts.Length);
        Assert.AreEqual(101, expirations[0].Contracts[0].OI);

        // Act
        await repository.CreateOrUpdateAsync(new Option
        {
            Ticker = Ticker,
            Expiration = Expiration,
            Contracts = new[]
            {
                new OptionContract
                {
                    Ticker = $"{Ticker}{Expiration}",
                    TimeStamp = DateTime.UtcNow,
                    OI = 202
                },
                new OptionContract
                {
                    Ticker = $"{Ticker}{Expiration}#1",
                    TimeStamp = DateTime.UtcNow,
                    OI = 303
                }
            }
        });

        // Assert
        expirations = (await repository.FindByTickerAsync(Ticker)).ToArray();

        Assert.AreEqual(1, expirations.Length);
        Assert.AreEqual(2, expirations[0].Contracts.Length);
        Assert.AreEqual(202, expirations[0].Contracts[0].OI);
        Assert.AreEqual(303, expirations[0].Contracts[1].OI);
    }
}