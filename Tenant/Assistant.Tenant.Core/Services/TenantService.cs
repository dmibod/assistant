namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Repositories;
using Common.Core.Security;
using Microsoft.Extensions.Logging;

public class TenantService : ITenantService
{
    private readonly ITenantRepository repository;
    private readonly IIdentityProvider provider;
    private readonly ILogger<TenantService> logger;

    public TenantService(ITenantRepository repository, IIdentityProvider provider, ILogger<TenantService> logger)
    {
        this.repository = repository;
        this.provider = provider;
        this.logger = logger;
    }

    public async Task<Tenant> GetOrCreateAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.GetOrCreateAsync));
        
        var identityName = this.provider.Identity.Name;
        
        if (!await this.repository.ExistsAsync(identityName))
        {
            await this.repository.CreateAsync(identityName);
        }

        return await this.repository.FindByNameAsync(identityName);
    }

    public async Task<string> EnsureExistsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.EnsureExistsAsync));
        
        var identityName = this.provider.Identity.Name;
        
        if (!await this.repository.ExistsAsync(identityName))
        {
            await this.repository.CreateAsync(identityName);
        }

        return identityName;
    }

    public Task<IEnumerable<string>> FindAllTenantsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindAllTenantsAsync));

        return this.repository.FindAllTenantsAsync();
    }
}