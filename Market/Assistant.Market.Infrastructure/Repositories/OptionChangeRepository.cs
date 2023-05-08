﻿namespace Assistant.Market.Infrastructure.Repositories;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Repositories;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

public class OptionChangeRepository : IOptionChangeRepository
{
    private readonly IMongoCollection<OptionChangeEntity> collection;
    private readonly ILogger<OptionChangeRepository> logger;

    public OptionChangeRepository(IOptions<DatabaseSettings> databaseSettings, ILogger<OptionChangeRepository> logger) :
        this(databaseSettings.Value, logger)
    {
    }

    public OptionChangeRepository(DatabaseSettings databaseSettings, ILogger<OptionChangeRepository> logger) :
        this(databaseSettings.ConnectionString,
            databaseSettings.DatabaseName,
            databaseSettings.OptionChangeCollectionName,
            logger)
    {
    }

    public OptionChangeRepository(string connectionString, string databaseName, string collectionName,
        ILogger<OptionChangeRepository> logger)
    {
        var mongoClient = new MongoClient(connectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        this.collection = mongoDatabase.GetCollection<OptionChangeEntity>(collectionName);
        this.logger = logger;
    }

    public async Task<IEnumerable<Option>> FindByTickerAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindByTickerAsync), ticker);

        return await this.collection.Find(entity => entity.Ticker == ticker).ToListAsync();
    }

    public async Task CreateOrUpdateAsync(Option option)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateOrUpdateAsync),
            $"{option.Ticker}-{option.Expiration}");

        if (await this.ExistsAsync(option.Ticker, option.Expiration))
        {
            await this.UpdateAsync(option);
        }
        else
        {
            await this.CreateAsync(option);
        }
    }

    private Task CreateAsync(Option option)
    {
        return this.collection.InsertOneAsync(option.AsChangeEntity());
    }

    private async Task UpdateAsync(Option option)
    {
        var filter = Builders<OptionChangeEntity>.Filter.Where(entity =>
            entity.Ticker == option.Ticker && entity.Expiration == option.Expiration);

        var tickers = option.Contracts.Select(contract => contract.Ticker).ToHashSet().ToArray();

        var builder = Builders<OptionChangeEntity>.Update;
        await this.collection.FindOneAndUpdateAsync(filter, builder.PullFilter(entity => entity.Contracts, Builders<OptionContract>.Filter.In(contract => contract.Ticker, tickers)));
        await this.collection.FindOneAndUpdateAsync(filter, builder.AddToSetEach(entity => entity.Contracts, option.Contracts).Set(entity => entity.LastRefresh, option.LastRefresh));
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
        var builder = Builders<OptionChangeEntity>.Filter;
        var filter = builder.Where(entity => entity.Ticker == ticker) & builder.In(entity => entity.Expiration, array);

        await this.collection.DeleteManyAsync(filter);
    }
    
    private Task<bool> ExistsAsync(string ticker, string expiration)
    {
        return this.collection.Find(entity => entity.Ticker == ticker && entity.Expiration == expiration).AnyAsync();
    }
    
    
    public async Task RemoveAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveAsync), ticker);

        var filter = Builders<OptionChangeEntity>.Filter.Where(entity => entity.Ticker == ticker);

        await this.collection.DeleteManyAsync(filter);
    }
}

internal class OptionChangeEntity : Option
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
}

internal static class OptionChangeExtensions
{
    public static OptionChangeEntity? AsChangeEntity(this Option? option)
    {
        if (option == null)
        {
            return null;
        }

        return new OptionChangeEntity
        {
            Ticker = option.Ticker,
            Expiration = option.Expiration,
            LastRefresh = option.LastRefresh,
            Contracts = option.Contracts.ToArray()
        };
    }
}