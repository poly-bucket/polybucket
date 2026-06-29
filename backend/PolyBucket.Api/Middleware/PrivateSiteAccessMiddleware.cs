using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Middleware;

public class PrivateSiteAccessMiddleware
{
    public const string MarkerHeader = "X-Site-Access";
    public const string MarkerValue = "login-required";

    private static readonly string[] AllowlistedPrefixes =
    {
        "/api/auth",
        "/api/systemsetup",
        "/api/system-settings/theme",
        "/api/system-settings/extensible-theme",
        "/health",
        "/swagger",
        "/openapi"
    };

    private readonly RequestDelegate _next;

    public PrivateSiteAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var privacy = context.RequestServices.GetRequiredService<ISitePrivacyState>();
        var allowPublicBrowsing = await privacy.IsPublicBrowsingAllowedAsync(context.RequestAborted);

        if (allowPublicBrowsing
            || context.User?.Identity?.IsAuthenticated == true
            || IsAllowlisted(context.Request.Path))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers[MarkerHeader] = MarkerValue;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Authentication required",
            Detail = "This site is private. Please sign in to continue."
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem), context.RequestAborted);
    }

    private static bool IsAllowlisted(PathString path)
    {
        foreach (var prefix in AllowlistedPrefixes)
        {
            if (path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
