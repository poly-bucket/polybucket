using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Files.Http;

public sealed class RequireModelDownloadAuthorizationFilter(
    ISiteModelDownloadPolicy downloadPolicy,
    IPermissionService permissionService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!await downloadPolicy.AreModelDownloadsEnabledAsync(context.HttpContext.RequestAborted))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Downloads disabled",
                Detail = "Model downloads are disabled on this site."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        var user = context.HttpContext.User;
        var userIdClaim = user.FindUserIdClaim();
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            var hasDownload = await permissionService.HasPermissionAsync(
                userId,
                PermissionConstants.MODEL_DOWNLOAD);
            if (!hasDownload)
            {
                context.Result = new ForbidResult();
            }

            return;
        }

        if (!await downloadPolicy.IsAnonymousModelDownloadAllowedAsync(context.HttpContext.RequestAborted))
        {
            context.Result = new UnauthorizedObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Sign in required",
                Detail = "Model downloads require a signed-in account on this site."
            });
        }
    }
}