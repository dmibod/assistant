namespace Assistant.Tenant.Infrastructure.Repositories;

using System.Runtime.InteropServices.JavaScript;
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

}

internal class TenantEntity : Tenant
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
}