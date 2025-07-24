using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PolyBucket.Api.Features.ACL.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ACL.Authorization
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequireResourceOwnershipAttribute(
        string resourceType,
        string ownPermission,
        string anyPermission,
        string ownerIdParameterName = "userId") : Attribute, IAsyncActionFilter
    {
        private readonly string _resourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
        private readonly string _ownPermission = ownPermission ?? throw new ArgumentNullException(nameof(ownPermission));
        private readonly string _anyPermission = anyPermission ?? throw new ArgumentNullException(nameof(anyPermission));
        private readonly string _ownerIdParameterName = ownerIdParameterName;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var permissionService = context.HttpContext.RequestServices.GetService(typeof(IPermissionService)) as IPermissionService;
            if (permissionService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // First check if user has "any" permission (can act on any resource)
            if (await permissionService.HasPermissionAsync(userId, _anyPermission))
            {
                await next();
                return;
            }

            // Check if user has "own" permission and owns the resource
            if (await permissionService.HasPermissionAsync(userId, _ownPermission))
            {
                // Extract owner ID from route parameters or request body
                Guid? resourceOwnerId = null;

                // Try to get from route parameters
                if (context.RouteData.Values.TryGetValue(_ownerIdParameterName, out var routeValue))
                {
                    if (Guid.TryParse(routeValue?.ToString(), out var parsedId))
                    {
                        resourceOwnerId = parsedId;
                    }
                }

                // Try to get from action parameters
                if (resourceOwnerId == null && context.ActionArguments.TryGetValue(_ownerIdParameterName, out var paramValue))
                {
                    if (paramValue is Guid guidValue)
                    {
                        resourceOwnerId = guidValue;
                    }
                    else if (Guid.TryParse(paramValue?.ToString(), out var parsedParam))
                    {
                        resourceOwnerId = parsedParam;
                    }
                }

                // Validate ownership
                if (resourceOwnerId.HasValue && 
                    await permissionService.ValidateUserCanPerformActionOnResourceAsync(userId, _ownPermission, _resourceType, resourceOwnerId.Value))
                {
                    await next();
                    return;
                }
            }

            context.Result = new ForbidResult();
        }
    }
} 