using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PolyBucket.Api.Features.ACL.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ACL.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _permissions;
        private readonly PermissionRequirement _requirement;

        public RequirePermissionAttribute(params string[] permissions)
        {
            _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
            _requirement = PermissionRequirement.All; // Default to requiring all permissions
        }

        public RequirePermissionAttribute(PermissionRequirement requirement, params string[] permissions)
        {
            _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
            _requirement = requirement;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var permissionService = context.HttpContext.RequestServices.GetService(typeof(IPermissionService)) as IPermissionService;
            if (permissionService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            var user = context.HttpContext.User;
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? user.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            bool hasPermission = _requirement switch
            {
                PermissionRequirement.All => await permissionService.HasAllPermissionsAsync(userId, _permissions),
                PermissionRequirement.Any => await permissionService.HasAnyPermissionAsync(userId, _permissions),
                _ => false
            };

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    public enum PermissionRequirement
    {
        All,
        Any
    }
} 