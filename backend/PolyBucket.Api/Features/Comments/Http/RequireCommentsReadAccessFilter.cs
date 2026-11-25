using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Comments.Http;

public sealed class RequireCommentsReadAccessFilter(ISiteModelFeaturePolicy featurePolicy) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!await featurePolicy.CanReadCommentsAsync(context.HttpContext.User, context.HttpContext.RequestAborted))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Comments unavailable",
                Detail = "Comments are not available on this site."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}