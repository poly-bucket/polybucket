using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Collections.Http;

public sealed class RequireCollectionsWriteAccessFilter(ISiteModelFeaturePolicy featurePolicy) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!await featurePolicy.CanWriteCollectionsAsync(context.HttpContext.User, context.HttpContext.RequestAborted))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Collections disabled",
                Detail = "Creating and editing collections is disabled on this site."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}