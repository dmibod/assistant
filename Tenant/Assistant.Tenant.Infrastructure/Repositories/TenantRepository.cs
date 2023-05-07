namespace Assistant.Tenant.Infrastructure.Repositories;

using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Repositories;
using Assistant.Tenant.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

public class TenantRepository : ITenantRepository
{
    private readonly IMongoCollection<TenantEntity> collection;
    private readonly ILogger<TenantRepository> logger;

    public TenantRepository(IOptions<DatabaseSettings> databaseSettings, ILogger<TenantRepository> logger)
    {
        var mongoClient = new MongoClient(
            databaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            databaseSettings.Value.DatabaseName);

        this.collection = mongoDatabase.GetCollection<TenantEntity>(databaseSettings.Value.TenantCollectionName);

        this.logger = logger;
    }

    public async Task<IEnumerable<string>> FindAllTenantsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindAllTenantsAsync));  
        
        return await this.collection.AsQueryable()
            .Select(entity => entity.Name)
            .ToListAsync();
    }

    public async Task<Tenant> FindByNameAsync(string name)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindByNameAsync), name);

        return await this.collection.Find(x => x.Name == name).FirstOrDefaultAsync();
    }

    public Task<bool> ExistsAsync(string name)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ExistsAsync), name);

        return this.collection.Find(doc => doc.Name == name).AnyAsync();
    }

    public Task CreateAsync(string name)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateAsync), name);

        return this.collection.InsertOneAsync(new TenantEntity
        {
            Name = name,
            WatchList = Array.Empty<WatchListItem>(),
            Positions = Array.Empty<Position>()
        });
    }

    public async Task<IEnumerable<Position>> FindPositionsAsync(string name)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindPositionsAsync), name);

        var tenant = await this.collection.Find(entity => entity.Name == name).FirstOrDefaultAsync();

        return tenant.Positions;
    }

    public async Task<IEnumerable<Position>> FindPositionsAsync(string tenant, Func<Position, bool> criteria)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindPositionsAsync), $"{tenant}");

        var t = await this.FindByNameAsync(tenant);

        return t == null ? null : t.Positions.Where(criteria);
    }

    public async Task<Position?> FindPositionAsync(string tenant, Func<Position, bool> criteria)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindPositionAsync), $"{tenant}");

        var t = await this.FindByNameAsync(tenant);

        return t == null ? null : t.Positions.FirstOrDefault(criteria);
    }

    public Task CreatePositionAsync(string tenant, Position position)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreatePositionAsync), $"{tenant}-{position.Account}-{position.Ticker}");
        
        var filter = Builders<TenantEntity>.Filter
            .Eq(tenant => tenant.Name, tenant);
        
        var update = Builders<TenantEntity>.Update
            .Push(tenant => tenant.Positions, position);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task UpdatePositionAsync(string tenant, Position position)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdatePositionAsync), $"{tenant}-{position.Account}-{position.Ticker}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) & 
                     Builders<TenantEntity>.Filter.ElemMatch(x => x.Positions, 
                         Builders<Position>.Filter.Eq(x => x.Account, position.Account) & 
                         Builders<Position>.Filter.Eq(x => x.Ticker, position.Ticker));
        
        var update = Builders<TenantEntity>.Update
            .Set("Positions.$.Quantity", position.Quantity)
            .Set("Positions.$.AverageCost", position.AverageCost);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task RemovePositionAsync(string tenant, string account, string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemovePositionAsync), $"{tenant}-{account}-{ticker}");

        var filter = Builders<TenantEntity>.Filter
            .Eq(tenant => tenant.Name, tenant);
        
        var update = Builders<TenantEntity>.Update
            .PullFilter(tenant => tenant.Positions, position => position.Account == account && position.Ticker == ticker);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task TagPositionAsync(string tenant, string account, string ticker, string tag)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.TagPositionAsync), $"{tenant}-{account}-{ticker}-{tag}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) & 
                     Builders<TenantEntity>.Filter.ElemMatch(x => x.Positions, 
                         Builders<Position>.Filter.Eq(x => x.Account, account) & 
                         Builders<Position>.Filter.Eq(x => x.Ticker, ticker));
        
        var update = Builders<TenantEntity>.Update.Set("Positions.$.Tag", tag);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task KanbanPositionAsync(string tenant, string account, string ticker, string cardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.KanbanPositionAsync), $"{tenant}-{account}-{ticker}-{cardId}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) & 
                     Builders<TenantEntity>.Filter.ElemMatch(x => x.Positions, 
                         Builders<Position>.Filter.Eq(x => x.Account, account) & 
                         Builders<Position>.Filter.Eq(x => x.Ticker, ticker));
        
        var update = Builders<TenantEntity>.Update.Set("Positions.$.CardId", cardId);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task<IEnumerable<WatchListItem>> FindWatchListAsync(string tenantName)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindWatchListAsync), tenantName);

        var tenant = await this.collection.Find(entity => entity.Name == tenantName).FirstOrDefaultAsync();
        
        return tenant.WatchList;
    }

    public Task CreateWatchListItemAsync(string tenant, WatchListItem listItem)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateWatchListItemAsync), $"{tenant}-{listItem.Ticker}");
        
        var filter = Builders<TenantEntity>.Filter
            .Eq(tenant => tenant.Name, tenant);
        
        var update = Builders<TenantEntity>.Update
            .Push(tenant => tenant.WatchList, listItem);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task RemoveWatchListItemAsync(string tenant, string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveWatchListItemAsync), $"{tenant}-{ticker}");

        var filter = Builders<TenantEntity>.Filter
            .Eq(tenant => tenant.Name, tenant);
        
        var update = Builders<TenantEntity>.Update
            .PullFilter(tenant => tenant.WatchList, item => item.Ticker == ticker);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task SetWatchListItemBuyPriceAsync(string tenant, string ticker, decimal price)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetWatchListItemBuyPriceAsync), $"{tenant}-{ticker}-{price}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) & 
                     Builders<TenantEntity>.Filter.ElemMatch(x => x.WatchList, 
                         Builders<WatchListItem>.Filter.Eq(x => x.Ticker, ticker));
        
        var update = Builders<TenantEntity>.Update.Set("WatchList.$.BuyPrice", price);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task SetWatchListItemSellPriceAsync(string tenant, string ticker, decimal price)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetWatchListItemSellPriceAsync), $"{tenant}-{ticker}-{price}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) & 
                     Builders<TenantEntity>.Filter.ElemMatch(x => x.WatchList, 
                         Builders<WatchListItem>.Filter.Eq(x => x.Ticker, ticker));
        
        var update = Builders<TenantEntity>.Update.Set("WatchList.$.SellPrice", price);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task SetWatchListItemPricesAsync(string tenant, string ticker, decimal buyPrice, decimal sellPrice)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetWatchListItemPricesAsync), $"{tenant}-{ticker}-{buyPrice}-{sellPrice}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) & 
                     Builders<TenantEntity>.Filter.ElemMatch(x => x.WatchList, 
                         Builders<WatchListItem>.Filter.Eq(x => x.Ticker, ticker));
        
        var update = Builders<TenantEntity>.Update
            .Set("WatchList.$.BuyPrice", buyPrice)
            .Set("WatchList.$.SellPrice", sellPrice);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task ResetTagAsync(string tenant)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ResetTagAsync), tenant);

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant);
        var update = Builders<TenantEntity>.Update.Set("Positions.$[].Tag", string.Empty);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task ReplaceTagAsync(string tenant, string oldValue, string newValue)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ReplaceTagAsync), $"{tenant}-{oldValue}-{newValue}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) & 
                     Builders<TenantEntity>.Filter.ElemMatch(x => x.Positions, 
                         Builders<Position>.Filter.Eq(x => x.Tag, oldValue));
        
        var update = Builders<TenantEntity>.Update.Set("Positions.$.Tag", newValue);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task<string?> FindDefaultFilterAsync(string tenant)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindDefaultFilterAsync), tenant);
        
        return await this.collection
            .AsQueryable()
            .Where(item => item.Name == tenant)
            .Select(item => item.DefaultFilter)
            .FirstOrDefaultAsync();
    }

    public Task UpdateDefaultFilterAsync(string tenant, string defaultFilter)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateDefaultFilterAsync), $"{tenant}-{defaultFilter}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant);
        var update = Builders<TenantEntity>.Update.Set(tenant => tenant.DefaultFilter, defaultFilter);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task<string?> FindPositionsBoardIdAsync(string tenant)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindPositionsBoardIdAsync), tenant);
        
        return await this.collection
            .AsQueryable()
            .Where(item => item.Name == tenant)
            .Select(item => item.PositionsBoardId)
            .FirstOrDefaultAsync();
    }

    public Task UpdatePositionsBoardIdAsync(string tenant, string positionsBoardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdatePositionsBoardIdAsync), $"{tenant}-{positionsBoardId}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant);
        var update = Builders<TenantEntity>.Update.Set(tenant => tenant.PositionsBoardId, positionsBoardId);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }
}

internal class TenantEntity : Tenant
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
}