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
        var tenant = await this.tenantService.GetOrCreateAsync();

        return tenant.Positions;
    }

    public Task<Position> CreateAsync(Position position)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(string account, string asset)
    {
        throw new NotImplementedException();
    }

    public Task ResetTagAsync()
    {
        throw new NotImplementedException();
    }

    public Task ReplaceTagAsync(string oldValue, string newValue)
    {
        throw new NotImplementedException();
    }

    public Task UpdateTagAsync(string account, string asset)
    {
        throw new NotImplementedException();
    }
}