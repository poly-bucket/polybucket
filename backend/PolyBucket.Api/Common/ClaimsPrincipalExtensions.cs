using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PolyBucket.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public static string? FindUserIdClaim(this ClaimsPrincipal? user)
    {
        if (user is null)
            return null;

        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? user.FindFirst("sub")?.Value;
    }
}
