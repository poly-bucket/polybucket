using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Middleware;

public class PrivateSiteFilesAccessMiddleware
{
    private readonly RequestDelegate _next;

    public PrivateSiteFilesAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/files", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var privacy = context.RequestServices.GetRequiredService<ISitePrivacyState>();
        var allowPublicBrowsing = await privacy.IsPublicBrowsingAllowedAsync(context.RequestAborted);

        if (PrivateSiteAccessEvaluator.ShouldAllowRequest(
                context.Request.Path,
                context.User?.Identity?.IsAuthenticated == true,
                allowPublicBrowsing))
        {
            await _next(context);
            return;
        }

        await PrivateSiteAccessResponseWriter.WriteUnauthorizedAsync(context);
    }
}
