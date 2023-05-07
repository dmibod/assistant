namespace Common.Core.Messaging.TenantResolver;

using Common.Core.Security;

public class CompositeTenantResolver : ITenantResolver
{
    private readonly IEnumerable<ITenantResolver> resolvers;

    public CompositeTenantResolver(params ITenantResolver[] resolvers)
    {
        this.resolvers = resolvers;
    }

    public string Resolve(object message)
    {
        foreach (var resolver in this.resolvers)
        {
            var tenant = resolver.Resolve(message);

            if (tenant != Identity.System)
            {
                return tenant;
            }
        }

        return Identity.System;
    }
}