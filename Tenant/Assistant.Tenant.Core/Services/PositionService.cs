namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Repositories;
using Microsoft.Extensions.Logging;

public class PositionService : IPositionService
{
    private readonly ITenantService tenantService;
    private readonly ITenantRepository repository;
    private readonly IMarketDataService marketDataService;
    private readonly ILogger<PositionService> logger;

    public PositionService(ITenantService tenantService, ITenantRepository repository, IMarketDataService marketDataService, ILogger<PositionService> logger)
    {
        this.tenantService = tenantService;
        this.repository = repository;
        this.marketDataService = marketDataService;
        this.logger = logger;
    }

    public async Task<IEnumerable<Position>> FindAllAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindAllAsync));
        
        var tenant = await this.tenantService.GetOrCreateAsync();

        return tenant.Positions;
    }

    public async Task<Position> CreateAsync(Position position)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateAsync), $"{position.Account}-{position.Ticker}");
        
        var tenant = await this.tenantService.GetOrCreateAsync();

        await this.repository.CreatePositionAsync(tenant.Name, position);

        if (position.Type == AssetType.Stock)
        {
            await this.marketDataService.EnsureStockAsync(position.Ticker);
        }

        return position;
    }

    public async Task RemoveAsync(string account, string ticker)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveAsync), $"{account}-{ticker}");

        var tenant = await this.tenantService.GetOrCreateAsync();
        
        await this.repository.RemovePositionAsync(tenant.Name, account, ticker);
    }

    public Task ResetTagAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.ResetTagAsync));

        throw new NotImplementedException();
    }

    public Task ReplaceTagAsync(string oldValue, string newValue)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ReplaceTagAsync), $"{oldValue}-{newValue}");

        throw new NotImplementedException();
    }

    public async Task UpdateTagAsync(string account, string ticker, string tag)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateTagAsync), $"{account}-{ticker}-{tag}");

        var tenant = await this.tenantService.GetOrCreateAsync();
        
        await this.repository.TagPositionAsync(tenant.Name, account, ticker, tag);
    }
}