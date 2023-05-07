namespace Common.Infrastructure.Security;

using System.Security.Principal;
using Common.Core.Security;

public class IdentityManager : IIdentityAccessor, IIdentityHolder
{
    public IIdentity Identity { get; set; } = null!;
}
