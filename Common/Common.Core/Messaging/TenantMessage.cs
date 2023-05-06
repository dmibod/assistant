namespace Common.Core.Messaging;

public class TenantMessage : ITenantAware
{
    public string Tenant { get; set; }
}