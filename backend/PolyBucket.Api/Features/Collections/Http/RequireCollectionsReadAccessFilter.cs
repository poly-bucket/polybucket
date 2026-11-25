using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Collections.Http;

public sealed class RequireCollectionsReadAccessFilter(ISiteModelFeaturePolicy featurePolicy) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!await featurePolicy.CanReadCollectionsAsync(context.HttpContext.User, context.HttpContext.RequestAborted))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Collections unavailable",
                Detail = "Collections are not available on this site."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}