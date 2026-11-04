using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Middleware;

public class PrivateSiteAccessMiddleware
{
    public const string MarkerHeader = "X-Site-Access";
    public const string MarkerValue = "login-required";

    private readonly RequestDelegate _next;

    public PrivateSiteAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
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
