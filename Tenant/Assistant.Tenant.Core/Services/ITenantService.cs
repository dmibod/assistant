namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface ITenantService
{
    Task<Tenant> GetOrCreateAsync();
}