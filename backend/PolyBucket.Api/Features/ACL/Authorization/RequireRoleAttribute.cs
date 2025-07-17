using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PolyBucket.Api.Features.ACL.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ACL.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _roles;
        private readonly RoleRequirement _requirement;

        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _requirement = RoleRequirement.Any; // Default to requiring any of the specified roles
        }

        public RequireRoleAttribute(RoleRequirement requirement, params string[] roles)
        {
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
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

            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRole = await permissionService.GetUserRoleAsync(userId);
            if (userRole == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Admins always pass role checks (supremacy rule)
            if (await permissionService.IsAdminAsync(userId))
            {
                return;
            }

            bool hasRole = _requirement switch
            {
                RoleRequirement.Any => _roles.Any(role => string.Equals(userRole.Name, role, StringComparison.OrdinalIgnoreCase)),
                RoleRequirement.Exact => _roles.Length == 1 && string.Equals(userRole.Name, _roles[0], StringComparison.OrdinalIgnoreCase),
                RoleRequirement.MinimumPriority => await HasMinimumRolePriorityAsync(permissionService, userId),
                _ => false
            };

            if (!hasRole)
            {
                context.Result = new ForbidResult();
            }
        }

        private async Task<bool> HasMinimumRolePriorityAsync(IPermissionService permissionService, Guid userId)
        {
            var userRole = await permissionService.GetUserRoleAsync(userId);
            if (userRole == null) return false;

            var allRoles = await permissionService.GetAllRolesAsync();
            var requiredRole = allRoles.FirstOrDefault(r => _roles.Contains(r.Name, StringComparer.OrdinalIgnoreCase));
            
            if (requiredRole == null) return false;

            return userRole.Priority >= requiredRole.Priority;
        }
    }

    public enum RoleRequirement
    {
        Any,
        Exact,
        MinimumPriority
    }
} 