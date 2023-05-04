namespace Common.Core.Security;

using System.Security.Principal;

public interface IIdentityAccessor
{
    IIdentity Identity { get; }
}