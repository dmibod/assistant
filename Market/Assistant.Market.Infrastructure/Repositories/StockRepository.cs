namespace Assistant.Market.Infrastructure.Repositories;

using Assistant.Market.Core.Models;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

public class StockRepository
{
    private readonly IMongoCollection<StockEntity> collection;
    private readonly ILogger<StockRepository> logger;

    public StockRepository(IOptions<DatabaseSettings> databaseSettings, ILogger<StockRepository> logger)
    {
        var mongoClient = new MongoClient(
            databaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            databaseSettings.Value.DatabaseName);

        this.collection = mongoDatabase.GetCollection<StockEntity>(
            databaseSettings.Value.StockCollectionName);

        this.logger = logger;
    }

    public async Task CreateAsync(StockEntity newItem) =>
        await this.collection.InsertOneAsync(newItem);

    public StockEntity? FindOutdatedWithLagAsync(TimeSpan lag)
    {
        this.logger.LogInformation("{Method} with lag {Argument}", nameof(this.FindOutdatedWithLagAsync),
            lag.ToString());

        var outdated = DateTime.UtcNow - lag;

        return this.collection
            .AsQueryable()
            .OrderBy(item => item.LastRefresh)
            .FirstOrDefault(item => item.LastRefresh < outdated);
    }
}

public class StockEntity : Stock
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
}