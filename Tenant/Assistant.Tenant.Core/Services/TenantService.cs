namespace Assistant.Tenant.Core.Services;

using System.Text.Json;
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

    public async Task<RecommendationFilter?> GetDefaultFilterAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.GetDefaultFilterAsync));

        var tenant = await this.EnsureExistsAsync();

        var filter = await this.repository.FindDefaultFilterAsync(tenant);

        return string.IsNullOrEmpty(filter) ? null : JsonSerializer.Deserialize<RecommendationFilter>(filter);
    }

    public async Task UpdateDefaultFilterAsync(RecommendationFilter filter)
    {
        this.logger.LogInformation("{Method}", nameof(this.GetDefaultFilterAsync));

        var tenant = await this.EnsureExistsAsync();

        await this.repository.UpdateDefaultFilterAsync(tenant, JsonSerializer.Serialize(filter));
    }
}