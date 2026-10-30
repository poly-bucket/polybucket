using PolyBucket.Api.Features.ACL.GetUserPermissions.Http;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.GetUserPermissions.Domain;

internal static class UserPermissionsResponseBuilder
{
    public static async Task<UserPermissionsResponseDto?> BuildAsync(Guid userId, IPermissionService permissionService)
    {
        var userRole = await permissionService.GetUserRoleAsync(userId);
        if (userRole == null)
        {
            return null;
        }

        var effectivePermissions = await permissionService.GetUserPermissionsAsync(userId);
        var permissionOverrides = await permissionService.GetUserPermissionOverridesAsync(userId);

        return new UserPermissionsResponseDto
        {
            UserId = userId,
            Role = new UserRoleSummaryDto
            {
                Id = userRole.Id,
                Name = userRole.Name,
                Priority = userRole.Priority
            },
            EffectivePermissions = effectivePermissions,
            PermissionOverrides = permissionOverrides.Select(po => new PermissionOverrideResponseDto
            {
                Id = po.Id,
                PermissionName = po.Permission.Name,
                IsGranted = po.IsGranted,
                Reason = po.Reason,
                ExpiresAt = po.ExpiresAt,
                GrantedByUsername = po.GrantedByUser?.Username,
                CreatedAt = po.CreatedAt
            }).ToList()
        };
    }
}
