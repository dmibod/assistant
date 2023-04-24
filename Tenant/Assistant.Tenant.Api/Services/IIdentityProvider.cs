namespace Assistant.Tenant.Api.Services;

using System.Security.Principal;
using Assistant.Tenant.Core.Security;

public class IdentityProvider : IIdentityProvider
{
    private readonly IHttpContextAccessor accessor;

    public IdentityProvider(IHttpContextAccessor accessor)
    {
        this.accessor = accessor;
    }

    public IIdentity Identity => this.accessor.HttpContext.User.Identity;
}

