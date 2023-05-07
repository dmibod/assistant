namespace Common.Core.Messaging.TenantResolver;

public interface ITenantResolver
{
    string Resolve(object message);
}