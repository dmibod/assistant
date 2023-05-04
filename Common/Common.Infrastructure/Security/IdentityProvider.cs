namespace Common.Infrastructure.Security;

using System.Security.Principal;
using Common.Core.Security;
using Microsoft.AspNetCore.Http;

public class IdentityProvider : IIdentityProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IIdentityAccessor identityAccessor;

    public IdentityProvider(IHttpContextAccessor httpContextAccessor, IIdentityAccessor identityAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.identityAccessor = identityAccessor;
    }

    public IIdentity Identity => this.httpContextAccessor.HttpContext?.User?.Identity ?? this.identityAccessor.Identity;
}