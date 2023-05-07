namespace Common.Core.Messaging.Models;

public class TenantMessage : ITenantAware
{
    public string Tenant { get; set; }
}