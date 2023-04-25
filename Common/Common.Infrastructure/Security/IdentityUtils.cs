namespace Common.Infrastructure.Security;

using System.Security.Claims;

public static class IdentityUtils
{
    public static DateTime GetExpiration(this ClaimsIdentity identity)
    {
        var claim = identity.Claims.Single(c => c.Type == "exp").Value;
        var value = long.Parse(claim);
        return DateTime.UnixEpoch.AddSeconds(value);
    }
}