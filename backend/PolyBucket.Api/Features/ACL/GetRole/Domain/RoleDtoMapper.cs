using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.GetRole.Domain;

internal static class RoleDtoMapper
{
    public static async Task<RoleDto> MapAsync(Role role, IRoleManagementService roleManagement)
    {
        var permissions = await roleManagement.GetRolePermissionsAsync(role.Id);
        var userCount = await roleManagement.GetUserCountAsync(role.Id);
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Priority = role.Priority,
            IsSystemRole = role.IsSystemRole,
            IsDefault = role.IsDefault,
            CanBeDeleted = role.CanBeDeleted,
            IsActive = role.IsActive,
            ParentRoleId = role.ParentRoleId,
            Permissions = permissions,
            UserCount = userCount,
            Color = role.Color
        };
    }
}
