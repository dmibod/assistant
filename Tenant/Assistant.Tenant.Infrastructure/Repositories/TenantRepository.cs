﻿namespace Assistant.Tenant.Infrastructure.Repositories;

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

    public async Task<bool> ExistsAsync(string name)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ExistsAsync), name);

        var exists = await this.collection.Find(doc => doc.Name == name).AnyAsync();

        if (exists)
        {
            await this.InitPositionsAsync(name);
            await this.InitWatchListAsync(name);
            await this.InitSchedulesAsync(name);
        }

        return exists;
    }

    public Task CreateAsync(string name)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateAsync), name);

        return this.collection.InsertOneAsync(new TenantEntity
        {
            Name = name,
            WatchList = Array.Empty<WatchListItem>(),
            Positions = Array.Empty<Position>(),
            Schedules = Array.Empty<Schedule>()
        });
    }

    public async Task<IEnumerable<Schedule>> FindSchedules(string name)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindSchedules), name);

        var tenant = await this.collection.Find(entity => entity.Name == name).FirstOrDefaultAsync();

        return tenant.Schedules;
    }

    public async Task<Schedule> FindSchedule(string tenant, ScheduleType scheduleType)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindSchedule), $"{tenant}-{scheduleType}");

        var t = await this.FindByNameAsync(tenant);

        return t == null ? null : t.Schedules.FirstOrDefault(schedule => schedule.ScheduleType == scheduleType);
    }

    public async Task<Schedule> CreateScheduleAsync(string tenant, ScheduleType scheduleType, ScheduleInterval interval)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateScheduleAsync), $"{tenant}-{scheduleType}-{interval}");

        var schedule = new Schedule{ ScheduleType = scheduleType, Interval = interval, LastExecution = DateTime.UnixEpoch };
        
        var filter = Builders<TenantEntity>.Filter
            .Eq(tenant => tenant.Name, tenant);

        var update = Builders<TenantEntity>.Update
            .Push(tenant => tenant.Schedules, schedule);
        
        await this.collection.FindOneAndUpdateAsync(filter, update);

        return schedule;
    }

    public Task UpdateSchedule(string tenant, ScheduleType scheduleType, ScheduleInterval interval)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateSchedule), $"{tenant}-{scheduleType}-{interval}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) &
                     Builders<TenantEntity>.Filter.ElemMatch(x => x.Schedules,
                         Builders<Schedule>.Filter.Eq(x => x.ScheduleType, scheduleType));
        
        var update = Builders<TenantEntity>.Update
            .Set("Schedules.$.Interval", interval);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    private Task InitSchedulesAsync(string tenant)
    {
        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) & 
                     Builders<TenantEntity>.Filter.Not(Builders<TenantEntity>.Filter.Exists(x => x.Schedules));
        var update = Builders<TenantEntity>.Update.Set(entity => entity.Schedules, Array.Empty<Schedule>());
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    private Task InitPositionsAsync(string tenant)
    {
        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) & 
                     Builders<TenantEntity>.Filter.Not(Builders<TenantEntity>.Filter.Exists(x => x.Positions));
        var update = Builders<TenantEntity>.Update.Set(entity => entity.Positions, Array.Empty<Position>());
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    private Task InitWatchListAsync(string tenant)
    {
        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) & 
                     Builders<TenantEntity>.Filter.Not(Builders<TenantEntity>.Filter.Exists(x => x.WatchList));
        var update = Builders<TenantEntity>.Update.Set(entity => entity.WatchList, Array.Empty<WatchListItem>());
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task ExecuteSchedule(string tenant, ScheduleType scheduleType)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ExecuteSchedule), $"{tenant}-{scheduleType}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) &
                     Builders<TenantEntity>.Filter.ElemMatch(x => x.Schedules,
                         Builders<Schedule>.Filter.Eq(x => x.ScheduleType, scheduleType));
        
        var update = Builders<TenantEntity>.Update
            .Set("Schedules.$.LastExecution", DateTime.UtcNow);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
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

    public async Task<IEnumerable<WatchListItem>> FindWatchListAsync(string tenant, Func<WatchListItem, bool> criteria)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindWatchListAsync), $"{tenant}");

        var t = await this.FindByNameAsync(tenant);

        return t == null ? null : t.WatchList.Where(criteria);
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

    public async Task<string?> FindSellPutsFilterAsync(string tenant)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindSellPutsFilterAsync), tenant);
        
        return await this.collection
            .AsQueryable()
            .Where(item => item.Name == tenant)
            .Select(item => item.DefaultFilter)
            .FirstOrDefaultAsync();
    }

    public Task UpdateSellPutsFilterAsync(string tenant, string defaultFilter)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateSellPutsFilterAsync), $"{tenant}-{defaultFilter}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant);
        var update = Builders<TenantEntity>.Update.Set(tenant => tenant.DefaultFilter, defaultFilter);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task<string?> FindSellCallsFilterAsync(string tenant)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindSellCallsFilterAsync), tenant);
        
        return await this.collection
            .AsQueryable()
            .Where(item => item.Name == tenant)
            .Select(item => item.SellCallsFilter)
            .FirstOrDefaultAsync();
    }

    public Task UpdateSellCallsFilterAsync(string tenant, string defaultFilter)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateSellCallsFilterAsync), $"{tenant}-{defaultFilter}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant);
        var update = Builders<TenantEntity>.Update.Set(tenant => tenant.SellCallsFilter, defaultFilter);
        
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

    public Task UpdatePositionsBoardIdAsync(string tenant, string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdatePositionsBoardIdAsync), $"{tenant}-{boardId}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant);
        var update = Builders<TenantEntity>.Update.Set(tenant => tenant.PositionsBoardId, boardId);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task<string?> FindSellPutsBoardIdAsync(string tenant)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindSellPutsBoardIdAsync), tenant);
        
        return await this.collection
            .AsQueryable()
            .Where(item => item.Name == tenant)
            .Select(item => item.SellPutsBoardId)
            .FirstOrDefaultAsync();
    }

    public Task UpdateSellPutsBoardIdAsync(string tenant, string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateSellPutsBoardIdAsync), $"{tenant}-{boardId}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant);
        var update = Builders<TenantEntity>.Update.Set(tenant => tenant.SellPutsBoardId, boardId);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task<string?> FindSellCallsBoardIdAsync(string tenant)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindSellCallsBoardIdAsync), tenant);
        
        return await this.collection
            .AsQueryable()
            .Where(item => item.Name == tenant)
            .Select(item => item.SellCallsBoardId)
            .FirstOrDefaultAsync();
    }

    public Task UpdateSellCallsBoardIdAsync(string tenant, string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateSellCallsBoardIdAsync), $"{tenant}-{boardId}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant);
        var update = Builders<TenantEntity>.Update.Set(tenant => tenant.SellCallsBoardId, boardId);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task<string?> FindOpenInterestFilterAsync(string tenant)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindOpenInterestFilterAsync), tenant);
        
        return await this.collection
            .AsQueryable()
            .Where(item => item.Name == tenant)
            .Select(item => item.OpenInterestFilter)
            .FirstOrDefaultAsync();
    }

    public Task UpdateOpenInterestFilterAsync(string tenant, string oiFilter)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateOpenInterestFilterAsync), $"{tenant}-{oiFilter}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant);
        var update = Builders<TenantEntity>.Update.Set(tenant => tenant.OpenInterestFilter, oiFilter);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task<string?> FindOpenInterestBoardIdAsync(string tenant)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindOpenInterestBoardIdAsync), tenant);
        
        return await this.collection
            .AsQueryable()
            .Where(item => item.Name == tenant)
            .Select(item => item.OpenInterestBoardId)
            .FirstOrDefaultAsync();
    }

    public Task UpdateOpenInterestBoardIdAsync(string tenant, string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateOpenInterestBoardIdAsync), $"{tenant}-{boardId}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant);
        var update = Builders<TenantEntity>.Update.Set(tenant => tenant.OpenInterestBoardId, boardId);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public Task UpdateWatchListBoardIdAsync(string tenant, string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateWatchListBoardIdAsync), $"{tenant}-{boardId}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant);
        var update = Builders<TenantEntity>.Update.Set(tenant => tenant.WatchListBoardId, boardId);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task<string?> FindWatchListBoardIdAsync(string tenant)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindWatchListBoardIdAsync), tenant);
        
        return await this.collection
            .AsQueryable()
            .Where(item => item.Name == tenant)
            .Select(item => item.WatchListBoardId)
            .FirstOrDefaultAsync();
    }

    public Task KanbanWatchListAsync(string tenant, string ticker, string cardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.KanbanWatchListAsync), $"{tenant}-{ticker}-{cardId}");

        var filter = Builders<TenantEntity>.Filter.Eq(tenant => tenant.Name, tenant) & 
                     Builders<TenantEntity>.Filter.ElemMatch(x => x.WatchList, Builders<WatchListItem>.Filter.Eq(x => x.Ticker, ticker));
        var update = Builders<TenantEntity>.Update.Set("WatchList.$.CardId", cardId);
        
        return this.collection.FindOneAndUpdateAsync(filter, update);
    }
}

internal class TenantEntity : Tenant
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
}