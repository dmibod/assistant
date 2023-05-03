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

public class StockRepository : IStockRepository
{
    private readonly IMongoCollection<StockEntity> collection;
    private readonly ILogger<StockRepository> logger;

    public StockRepository(IOptions<DatabaseSettings> databaseSettings, ILogger<StockRepository> logger) :
        this(databaseSettings.Value, logger)
    {
    }
    
    public StockRepository(DatabaseSettings databaseSettings, ILogger<StockRepository> logger) :
        this(databaseSettings.ConnectionString,
            databaseSettings.DatabaseName,
            databaseSettings.StockCollectionName,
            logger)
    {
    }

    public StockRepository(string connectionString, string databaseName, string collectionName,
        ILogger<StockRepository> logger)
    {
        var mongoClient = new MongoClient(connectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        this.collection = mongoDatabase.GetCollection<StockEntity>(collectionName);
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
            .Set(item => item.TimeStamp, stock.TimeStamp)
            .Set(item => item.LastRefresh, stock.LastRefresh);

        return this.collection.UpdateOneAsync(filter, update);
    }

    public async Task<string?> FindOutdatedTickerAsync(TimeSpan olderThan)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindOutdatedTickerAsync),
            olderThan.ToString());

        var threshold = DateTime.UtcNow - olderThan;

        return await this.collection
            .AsQueryable()
            .Where(item => item.LastRefresh < threshold)
            .OrderBy(item => item.LastRefresh)
            .Select(item => item.Ticker)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Stock>> FindAllAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindAllAsync));

        var list = await this.collection.Find(_ => true).ToListAsync();

        return list;
    }

    public async Task<IEnumerable<string>> FindTickersAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindTickersAsync));

        var list = await this.collection.AsQueryable().Select(doc => doc.Ticker).ToListAsync();

        return list;
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
            TimeStamp = stock.TimeStamp,
            LastRefresh = stock.LastRefresh
        };
    }
}