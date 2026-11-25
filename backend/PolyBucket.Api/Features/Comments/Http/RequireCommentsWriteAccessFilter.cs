using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Comments.Http;

public sealed class RequireCommentsWriteAccessFilter(ISiteModelFeaturePolicy featurePolicy) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!await featurePolicy.CanWriteCommentsAsync(context.HttpContext.User, context.HttpContext.RequestAborted))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Comments disabled",
                Detail = "Posting and editing comments is disabled on this site."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}