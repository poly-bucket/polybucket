using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Authentication.Authorization;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Models.CreateModel.Http;

public sealed class RequireModelUploadAuthorizationFilter(
    ISiteModelUploadPolicy uploadPolicy,
    IPermissionService permissionService,
    IEmailVerificationGate emailVerificationGate) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var userIdClaim = user.FindUserIdClaim();
        Guid? userId = null;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        if (userId.HasValue)
        {
            var hasCreate = await permissionService.HasPermissionAsync(userId.Value, PermissionConstants.MODEL_CREATE);
            if (!hasCreate)
            {
                context.Result = new ForbidResult();
                return;
            }

            if (!await emailVerificationGate.IsSatisfiedAsync(user, context.HttpContext.RequestAborted))
            {
                context.Result = RequireVerifiedEmailAttribute.CreateForbiddenResult();
            }

            return;
        }

        if (!await uploadPolicy.IsAnonymousModelUploadAllowedAsync(context.HttpContext.RequestAborted))
        {
            context.Result = new UnauthorizedObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Sign in required",
                Detail = "Model uploads require a signed-in account on this site."
            });
        }
    }
}