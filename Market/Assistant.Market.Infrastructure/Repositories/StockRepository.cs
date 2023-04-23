namespace Assistant.Market.Infrastructure.Repositories;

using Assistant.Market.Core.Models;
using Assistant.Market.Core.Repositories;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

public class StockRepository : IStockRepository
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

    public Task<bool> ExistsAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ExistsAsync), ticker);

        return this.collection.Find(doc => doc.Ticker == ticker).AnyAsync();
    }

    public async Task<Stock?> FindByTickerAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindByTickerAsync), ticker);

        return await this.collection.Find(x => x.Ticker == ticker).FirstOrDefaultAsync();
    }

    public Task CreateAsync(Stock stock)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateAsync), stock.Ticker);

        return this.collection.InsertOneAsync(stock.AsEntity());
    }

    public Task UpdateAsync(Stock stock)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateAsync), stock.Ticker);

        var filter = Builders<StockEntity>.Filter
            .Eq(item => item.Ticker, stock.Ticker);

        var update = Builders<StockEntity>.Update
            .Set(item => item.Ask, stock.Ask)
            .Set(item => item.Bid, stock.Bid)
            .Set(item => item.Last, stock.Last)
            .Set(item => item.LastRefresh, stock.LastRefresh);

        return this.collection.UpdateOneAsync(filter, update);
    }

    public Task<Stock?> FindOldestAsync(TimeSpan olderThan)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindOldestAsync),
            olderThan.ToString());

        var outdated = DateTime.UtcNow - olderThan;

        var entity = this.collection
            .AsQueryable()
            .OrderBy(item => item.LastRefresh)
            .FirstOrDefault(item => item.LastRefresh < outdated);

        return Task.FromResult(entity as Stock);
    }

    public async Task<IEnumerable<Stock>> FindAllAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindAllAsync));

        var list = await this.collection.Find(_ => true).ToListAsync();

        return list;
    }

    public Task<IEnumerable<string>> FindTickersAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindTickersAsync));

        return Task.FromResult(this.collection.AsQueryable().Select(doc => doc.Ticker).AsEnumerable());
    }
}

internal class StockEntity : Stock
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
}

internal static class StockExtensions
{
    public static StockEntity? AsEntity(this Stock? stock)
    {
        if (stock == null)
        {
            return null;
        }

        return new StockEntity
        {
            Ask = stock.Ask,
            Bid = stock.Bid,
            Last = stock.Last,
            Ticker = stock.Ticker,
            LastRefresh = stock.LastRefresh
        };
    }
}