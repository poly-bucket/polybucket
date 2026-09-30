using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PolyBucket.Tests.Features.Notifications;

internal static class NotificationControllerTestContext
{
    public static ControllerContext For(Guid? userId)
    {
        var claims = userId.HasValue ? new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) } : Array.Empty<Claim>();
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, userId.HasValue ? "Test" : null))
            }
        };
    }
}
