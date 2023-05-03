namespace Assistant.Market.Infrastructure.Repositories;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Repositories;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

public class OptionRepository : IOptionRepository
{
    private readonly IMongoCollection<OptionEntity> collection;
    private readonly ILogger<OptionRepository> logger;

    public OptionRepository(IOptions<DatabaseSettings> databaseSettings, ILogger<OptionRepository> logger) :
        this(databaseSettings.Value, logger)
    {
    }

    public OptionRepository(DatabaseSettings databaseSettings, ILogger<OptionRepository> logger) :
        this(databaseSettings.ConnectionString,
            databaseSettings.DatabaseName,
            databaseSettings.OptionCollectionName,
            logger)
    {
    }

    public OptionRepository(string connectionString, string databaseName, string collectionName,
        ILogger<OptionRepository> logger)
    {
        var mongoClient = new MongoClient(connectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        this.collection = mongoDatabase.GetCollection<OptionEntity>(collectionName);
        this.logger = logger;
    }

    public Task<bool> ExistsAsync(string ticker, string expiration)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ExistsAsync),
            $"{ticker}-{expiration}");

        return this.collection.Find(entity => entity.Ticker == ticker && entity.Expiration == expiration).AnyAsync();
    }

    public Task UpdateAsync(Option option)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateAsync),
            $"{option.Ticker}-{option.Expiration}");

        var filter = Builders<OptionEntity>.Filter.Where(entity =>
            entity.Ticker == option.Ticker && entity.Expiration == option.Expiration);
        var update = Builders<OptionEntity>.Update
            .Set(entity => entity.Contracts, option.Contracts)
            .Set(entity => entity.LastRefresh, option.LastRefresh);

        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task CreateAsync(Option option)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateAsync),
            $"{option.Ticker}-{option.Expiration}");

        return this.collection.InsertOneAsync(option.AsEntity());
    }

    public async Task<IEnumerable<Option>> FindByTickerAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindByTickerAsync), ticker);

        return await this.collection.Find(entity => entity.Ticker == ticker).ToListAsync();
    }

    public async Task<IEnumerable<string>> FindExpirationsAsync(string ticker)
    {
        this.logger.LogInformation("{Method}", nameof(this.FindExpirationsAsync));

        return await this.collection.AsQueryable()
            .Where(entity => entity.Ticker == ticker)
            .Select(entity => entity.Expiration)
            .ToListAsync();
    }

    public async Task RemoveAsync(IDictionary<string, ISet<string>> expirations)
    {
        this.logger.LogInformation("{Method}", nameof(this.RemoveAsync));
        
        foreach (var expiration in expirations)
        {
            await this.RemoveAsync(expiration.Key, expiration.Value);
        }
    }

    private async Task RemoveAsync(string ticker, ISet<string> expirations)
    {
        var array = expirations.ToArray();
        var builder = Builders<OptionEntity>.Filter;
        var filter = builder.Where(entity => entity.Ticker == ticker) & builder.In(entity => entity.Expiration, array);

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
            LastRefresh = option.LastRefresh,
            Contracts = option.Contracts.ToArray()
        };
    }
}