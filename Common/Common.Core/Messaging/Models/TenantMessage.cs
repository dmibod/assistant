namespace Common.Core.Messaging.Models;

public abstract class TenantMessage : ITenantAware
{
    public string Tenant { get; set; }
}