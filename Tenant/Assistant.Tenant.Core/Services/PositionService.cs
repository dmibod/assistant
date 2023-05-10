namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Repositories;
using Helper.Core.Utils;
using Microsoft.Extensions.Logging;

public class PositionService : IPositionService
{
    private readonly ITenantService tenantService;
    private readonly ITenantRepository repository;
    private readonly IMarketDataService marketDataService;
    private readonly INotificationService notificationService;
    private readonly ILogger<PositionService> logger;

    public PositionService(
        ITenantService tenantService, 
        ITenantRepository repository, 
        IMarketDataService marketDataService, 
        INotificationService notificationService, 
        ILogger<PositionService> logger)
    {
        this.tenantService = tenantService;
        this.repository = repository;
        this.marketDataService = marketDataService;
        this.notificationService = notificationService;
        this.logger = logger;
    }

    public async Task<IEnumerable<Position>> FindAllAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindAllAsync));
        
        var tenant = await this.tenantService.EnsureExistsAsync();

        return await this.repository.FindPositionsAsync(tenant);
    }

    public async Task<IEnumerable<Position>> FindByCardIdAsync(string cardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindByCardIdAsync), cardId);
        
        var tenant = await this.tenantService.EnsureExistsAsync();

        return await this.repository.FindPositionsAsync(tenant, position => position.CardId == cardId);
    }

    public async Task<Position> CreateAsync(Position position)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateAsync), $"{position.Account}-{position.Ticker}");
        
        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.CreatePositionAsync(tenant, position);

        if (position.Type == AssetType.Stock)
        {
            await this.marketDataService.EnsureStockAsync(position.Ticker);
        }

        await this.RefreshNotificationAsync();

        return position;
    }

    public async Task<Position> CreateOrUpdateAsync(Position position)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateOrUpdateAsync), $"{position.Account}-{position.Ticker}");

        var tenant = await this.tenantService.EnsureExistsAsync();
        
        var existing = await this.repository.FindPositionAsync(tenant, p => p.Account == position.Account && p.Ticker == position.Ticker);

        if (existing != null)
        {
            await this.repository.UpdatePositionAsync(tenant, position);
            
            await this.RefreshNotificationAsync();
            
            return existing;
        }

        await this.repository.CreatePositionAsync(tenant, position);

        if (position.Type == AssetType.Stock)
        {
            await this.marketDataService.EnsureStockAsync(position.Ticker);
        }
        
        await this.RefreshNotificationAsync();

        return position;
    }

    public async Task UpdateAsync(string account, string ticker, int quantity, decimal averageCost)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateAsync), $"{account}-{ticker}-{quantity}-{averageCost}");

        var tenant = await this.tenantService.EnsureExistsAsync();

        var position = new Position
        {
            Account = account,
            Ticker = ticker,
            Quantity = quantity,
            AverageCost = averageCost
        };
        
        await this.repository.UpdatePositionAsync(tenant, position);

        await this.RefreshNotificationAsync();
    }

    public async Task UpdateTagAsync(string account, string ticker, string tag)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateTagAsync), $"{account}-{ticker}-{tag}");

        var tenant = await this.tenantService.EnsureExistsAsync();
        
        await this.repository.TagPositionAsync(tenant, account, ticker, tag);

        await this.RefreshNotificationAsync();
    }

    public async Task UpdateCardIdAsync(string account, string ticker, string cardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateCardIdAsync), $"{account}-{ticker}-{cardId}");

        var tenant = await this.tenantService.EnsureExistsAsync();
        
        await this.repository.KanbanPositionAsync(tenant, account, ticker, cardId);
    }

    public async Task RemoveAsync(string account, string ticker, bool suppressNotifications)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveAsync), $"{account}-{ticker}-{suppressNotifications}");

        var tenant = await this.tenantService.EnsureExistsAsync();
        
        await this.repository.RemovePositionAsync(tenant, account, ticker);

        if (!suppressNotifications)
        {
            await this.RefreshNotificationAsync();
        }
    }

    public async Task ResetTagAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.ResetTagAsync));

        var tenant = await this.tenantService.EnsureExistsAsync();
        
        await this.repository.ResetTagAsync(tenant);
        
        await this.RefreshNotificationAsync();
    }

    public async Task ReplaceTagAsync(string oldValue, string newValue)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ReplaceTagAsync), $"{oldValue}-{newValue}");

        var tenant = await this.tenantService.EnsureExistsAsync();
        
        await this.repository.ReplaceTagAsync(tenant, oldValue, newValue);
        
        await this.RefreshNotificationAsync();
    }

    public async Task AutoTagOptionsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.AutoTagOptionsAsync));
        
        var tenant = await this.tenantService.EnsureExistsAsync();

        var positions = await this.repository.FindPositionsAsync(tenant);

        var groups = positions
            .Where(p => string.IsNullOrEmpty(p.Tag) && p.Type == AssetType.Option)
            .GroupBy(p => $"{p.Account}-{OptionUtils.GetStock(p.Ticker)}-{OptionUtils.GetExpiration(p.Ticker)}");

        var publishRefreshNotification = false;
        
        foreach (var group in groups.Where(g => g.Count() > 1))
        {
            foreach (var position in group)
            {
                await this.repository.TagPositionAsync(tenant, position.Account, position.Ticker, group.Key);

                publishRefreshNotification = true;
            }
        }

        if (publishRefreshNotification)
        {
            await this.RefreshNotificationAsync();
        }
    }

    public async Task<string?> FindPositionsBoardId()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindPositionsBoardId));

        var tenant = await this.tenantService.EnsureExistsAsync();

        return await this.repository.FindPositionsBoardIdAsync(tenant);
    }

    public async Task UpdatePositionsBoardId(string positionsBoardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdatePositionsBoardId), positionsBoardId);

        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.UpdatePositionsBoardIdAsync(tenant, positionsBoardId);
    }

    private Task RefreshNotificationAsync()
    {
        return this.notificationService.NotifyRefreshPositionsAsync();
    }
}