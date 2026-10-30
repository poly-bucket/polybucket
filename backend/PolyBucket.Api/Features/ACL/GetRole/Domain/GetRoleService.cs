using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.GetRole.Domain;

public class GetRoleService(IRoleManagementService roleManagement) : IGetRoleService
{
    public async Task<RoleDto?> GetRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await roleManagement.GetRoleByIdAsync(id);
        if (role == null)
        {
            return null;
        }

        return await RoleDtoMapper.MapAsync(role, roleManagement);
    }
}
