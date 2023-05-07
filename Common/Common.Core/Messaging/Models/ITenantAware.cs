namespace Common.Core.Messaging.Models;

public interface ITenantAware
{
    public string Tenant { get; }
}