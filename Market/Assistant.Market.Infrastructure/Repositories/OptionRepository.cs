namespace Assistant.Market.Infrastructure.Repositories;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Repositories;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

public class OptionRepository : IOptionRepository
{
    private readonly IMongoCollection<OptionEntity> collection;
    private readonly ILogger<OptionRepository> logger;

    public OptionRepository(IOptions<DatabaseSettings> databaseSettings, ILogger<OptionRepository> logger)
    {
        var mongoClient = new MongoClient(
            databaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            databaseSettings.Value.DatabaseName);

        this.collection = mongoDatabase.GetCollection<OptionEntity>(
            databaseSettings.Value.OptionCollectionName);

        this.logger = logger;
    }

    public Task<bool> ExistsAsync(string ticker, string expiration)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ExistsAsync),
            $"{ticker}-{expiration}");

        return this.collection.Find(doc => doc.Ticker == ticker && doc.Expiration == expiration).AnyAsync();
    }

    public Task UpdateAsync(Option option)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateAsync),
            $"{option.Ticker}-{option.Expiration}");

        return this.collection.ReplaceOneAsync(
            doc => doc.Ticker == option.Ticker && doc.Expiration == option.Expiration, option.AsEntity());
    }

    public Task CreateAsync(Option option)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateAsync),
            $"{option.Ticker}-{option.Expiration}");

        return this.collection.InsertOneAsync(option.AsEntity());
    }

    public Task<IEnumerable<Option>> FindByTickerAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindByTickerAsync), ticker);

        var options = this.collection.AsQueryable().Where(doc => doc.Ticker == ticker).ToList();

        return Task.FromResult(options.Cast<Option>());
    }

    public Task<IEnumerable<string>> FindExpirationsAsync(string ticker)
    {
        this.logger.LogInformation("{Method}", nameof(this.FindExpirationsAsync));

        return Task.FromResult(this.collection.AsQueryable().Where(doc => doc.Ticker == ticker).Select(doc => doc.Expiration).AsEnumerable());

    }

    public async Task RemoveAsync(IDictionary<string, ISet<string>> expirations)
    {
        this.logger.LogInformation("{Method}", nameof(this.RemoveAsync));

        var filter = Builders<OptionEntity>.Filter.Where(doc => expirations.ContainsKey(doc.Ticker) && expirations[doc.Ticker].Contains(doc.Expiration));

        await this.collection.DeleteManyAsync(filter);
    }
}

internal class OptionEntity : Option
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
}

internal static class OptionExtensions
{
    public static OptionEntity? AsEntity(this Option? option)
    {
        if (option == null)
        {
            return null;
        }

        return new OptionEntity
        {
            Ticker = option.Ticker,
            Expiration = option.Expiration,
            Contracts = option.Contracts.ToArray()
        };
    }
}