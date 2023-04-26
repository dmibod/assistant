namespace Assistant.Tenant.Infrastructure.Repositories;

using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Repositories;
using Assistant.Tenant.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

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

    public Task CreatePositionAsync(string tenant, Position position)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreatePositionAsync), $"{tenant}-{position.Account}-{position.Ticker}");
        
        var filter = Builders<TenantEntity>.Filter
            .Eq(tenant => tenant.Name, tenant);
        
        var update = Builders<TenantEntity>.Update
            .Push(tenant => tenant.Positions, position);
        
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
}

internal class TenantEntity : Tenant
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
}