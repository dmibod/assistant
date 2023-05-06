namespace Assistant.Tenant.Infrastructure.Services;

using Assistant.Tenant.Core.Services;
using Assistant.Tenant.Infrastructure.Configuration;
using Common.Core.Messaging;
using Common.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

public class MarketDataService : IMarketDataService
{
    private readonly IMongoCollection<AssetPriceEntity> stockCollection;
    
    private readonly IMongoCollection<OptionPriceEntity> optionCollection;

    private readonly string stockCreateTopic;

    private readonly IBusService busService;
    
    private readonly ILogger<MarketDataService> logger;

    public MarketDataService(IBusService busService, ITopicResolver topicResolver, IOptions<DatabaseSettings> databaseSettings, ILogger<MarketDataService> logger)
    {
        this.stockCreateTopic = topicResolver.Resolve("{StockCreateTopic}");
        
        var mongoClient = new MongoClient(
            databaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            databaseSettings.Value.DatabaseName);

        this.stockCollection = mongoDatabase.GetCollection<AssetPriceEntity>("stock");
        
        this.optionCollection = mongoDatabase.GetCollection<OptionPriceEntity>("option");

        this.busService = busService;
        this.logger = logger;
    }

    public Task EnsureStockAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.EnsureStockAsync), ticker);
        
        return this.busService.PublishAsync(this.stockCreateTopic, new StockMessage { Ticker = ticker });
    }

    public async Task<IEnumerable<AssetPrice>> FindStockPricesAsync(ISet<string> tickers)
    {
        this.logger.LogInformation("{Method}", nameof(this.FindStockPricesAsync));

        var cursor = await this.stockCollection.FindAsync(doc => tickers == null || tickers.Contains(doc.Ticker));

        return cursor.ToEnumerable().ToList();
    }

    public async Task<IEnumerable<OptionAssetPrice>> FindOptionPricesAsync(string stockTicker, string expiration)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindOptionPricesAsync), $"{stockTicker}-{expiration}");
        
        var cursor = await this.optionCollection.FindAsync(doc => doc.Ticker == stockTicker && doc.Expiration == expiration);

        return cursor.ToEnumerable().SelectMany(doc => doc.Contracts).ToList();
    }

    public async Task<IEnumerable<string>> FindExpirationsAsync(string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindExpirationsAsync), ticker);

        var cursor = await this.optionCollection.FindAsync(doc => doc.Ticker == ticker);

        return cursor.ToEnumerable().Select(doc => doc.Expiration).ToList();
    }
}

internal class AssetPriceEntity : AssetPrice
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public DateTime LastRefresh { get; set; }
}

internal class OptionPriceEntity : OptionPrice
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public DateTime LastRefresh { get; set; }
}
