namespace Common.Core.Messaging;

public interface ITenantAware
{
    public string Tenant { get; }
}