namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface ITenantService
{
    Task<Tenant> GetOrCreateAsync();

    Task<string> EnsureExistsAsync();

    Task<SuggestionFilter?> GetDefaultFilterAsync();

    Task UpdateDefaultFilterAsync(SuggestionFilter filter);
}