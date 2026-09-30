using System;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Common;

namespace PolyBucket.Api.Features.Models.Common;

public static class ModelEngagementViewerKey
{
    public static string Build(HttpContext httpContext, ClaimsPrincipal user)
    {
        if (TryResolveUserId(user, out var userId))
        {
            return $"u:{userId}";
        }

        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var hash = Hash($"{ip}|{userAgent}");
        return $"a:{hash}";
    }

    public static bool TryResolveUserId(ClaimsPrincipal user, out Guid userId)
    {
        var userIdClaim = user.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
        {
            userId = default;
            return false;
        }

        return true;
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes, 0, 8).ToLowerInvariant();
    }
}
