namespace Assistant.Tenant.Core.Repositories;

using Assistant.Tenant.Core.Models;

public interface ITenantRepository
{
    Task<Tenant> FindByNameAsync(string name);

    Task<bool> ExistsAsync(string name);
    
    Task CreateAsync(string name);
}