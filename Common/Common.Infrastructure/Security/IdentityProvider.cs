namespace Common.Infrastructure.Security;

using System.Security.Principal;
using Common.Core.Security;
using Microsoft.AspNetCore.Http;

public class IdentityProvider : IIdentityProvider
{
    private static readonly IIdentity DefaultIdentity = new Identity
    {
        Name = "anonymous",
        AuthenticationType = string.Empty,
        IsAuthenticated = false
    };
    
    private readonly IHttpContextAccessor accessor;

    public IdentityProvider(IHttpContextAccessor accessor)
    {
        this.accessor = accessor;
    }

    public IIdentity Identity => this.accessor.HttpContext?.User?.Identity ?? DefaultIdentity;
}

internal class Identity : IIdentity
{
    public string? AuthenticationType { get; set; }
    public bool IsAuthenticated { get; set; }
    public string? Name { get; set; }
}

