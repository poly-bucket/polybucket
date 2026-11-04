using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PolyBucket.Api.Middleware;

public static class PrivateSiteAccessResponseWriter
{
    public static async Task WriteUnauthorizedAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers[PrivateSiteAccessMiddleware.MarkerHeader] =
            PrivateSiteAccessMiddleware.MarkerValue;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Authentication required",
            Detail = "This site is private. Please sign in to continue."
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem), context.RequestAborted);
    }
}
