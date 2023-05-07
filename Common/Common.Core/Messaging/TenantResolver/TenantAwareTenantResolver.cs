namespace Common.Core.Messaging.TenantResolver;

using Common.Core.Messaging.Models;
using Common.Core.Security;

public class TenantAwareTenantResolver : ITenantResolver
{
    public string Resolve(object message)
    {
        var tenantAware = message as ITenantAware;
        
        return tenantAware == null ? Identity.System : tenantAware.Tenant;
    }
}