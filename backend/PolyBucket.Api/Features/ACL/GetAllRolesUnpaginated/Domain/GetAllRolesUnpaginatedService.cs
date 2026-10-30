using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.GetAllRolesUnpaginated.Domain;

public interface IGetAllRolesUnpaginatedService
{
    Task<List<RoleDto>> GetAllRolesUnpaginatedAsync(CancellationToken cancellationToken = default);
}

public class GetAllRolesUnpaginatedService(IRoleManagementService roleManagement) : IGetAllRolesUnpaginatedService
{
    public async Task<List<RoleDto>> GetAllRolesUnpaginatedAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleManagement.GetAllRolesAsync();
        var roleDtos = new List<RoleDto>();
        foreach (var role in roles)
        {
            var permissions = await roleManagement.GetRolePermissionsAsync(role.Id);
            var userCount = await roleManagement.GetUserCountAsync(role.Id);
            roleDtos.Add(new RoleDto
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
            });
        }

        return roleDtos;
    }
}
