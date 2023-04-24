namespace Assistant.Tenant.Infrastructure.Configuration;

public class DatabaseSettings : Common.Infrastructure.Configuration.DatabaseSettings
{
    public string TenantCollectionName { get; set; } = null!;

}