namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Repositories;
using Microsoft.Extensions.Logging;

public class PositionService : IPositionService
{
    private readonly ITenantService tenantService;
    private readonly ITenantRepository repository;
    private readonly ILogger<PositionService> logger;

    public PositionService(ITenantService tenantService, ITenantRepository repository, ILogger<PositionService> logger)
    {
        this.tenantService = tenantService;
        this.repository = repository;
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
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateAsync), $"{position.Account}-{position.Asset}");
        
        var tenant = await this.tenantService.GetOrCreateAsync();

        await this.repository.CreatePositionAsync(tenant.Name, position);

        return position;
    }

    public Task RemoveAsync(string account, string asset)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveAsync), $"{account}-{asset}");
        
        throw new NotImplementedException();
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

    public Task UpdateTagAsync(string account, string asset)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateTagAsync), $"{account}-{asset}");

        throw new NotImplementedException();
    }
}