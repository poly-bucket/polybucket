using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.GetRole.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.UpdateRole.Http;

namespace PolyBucket.Api.Features.ACL.UpdateRole.Domain;

public class UpdateRoleService(IRoleManagementService roleManagement) : IUpdateRoleService
{
    public async Task<(UpdateRoleOutcome Outcome, RoleDto? Role, string? Error)> UpdateRoleAsync(
        Guid actorUserId, Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty)
        {
            return (UpdateRoleOutcome.Unauthorized, null, null);
        }

        if (!await roleManagement.CanManageRoleAsync(actorUserId, roleId))
        {
            return (UpdateRoleOutcome.Forbidden, null, "Insufficient privileges to manage this role");
        }

        try
        {
            var existing = await roleManagement.GetRoleByIdAsync(roleId);
            if (existing == null)
            {
                return (UpdateRoleOutcome.NotFound, null, "Role not found");
            }

            var updated = await roleManagement.UpdateRoleAsync(roleId, request);
            var dto = await RoleDtoMapper.MapAsync(updated, roleManagement);
            return (UpdateRoleOutcome.Success, dto, null);
        }
        catch (InvalidOperationException ex)
        {
            return (UpdateRoleOutcome.Invalid, null, ex.Message);
        }
        catch (Exception ex)
        {
            return (UpdateRoleOutcome.Invalid, null, $"Failed to update role: {ex.Message}");
        }
    }
}
