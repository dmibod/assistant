namespace Common.Core.Security;

using System.Security.Principal;

public interface IIdentityProvider
{
    IIdentity Identity { get; }
}