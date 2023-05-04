namespace Common.Core.Security;

using System.Security.Principal;

public interface IIdentityHolder
{
    IIdentity Identity { set; }
}