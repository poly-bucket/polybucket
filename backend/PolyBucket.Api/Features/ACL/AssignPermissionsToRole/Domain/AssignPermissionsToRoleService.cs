using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.AssignPermissionsToRole.Domain;

public class AssignPermissionsToRoleService(IRoleManagementService roleManagement) : IAssignPermissionsToRoleService
{
    public async Task<(AssignPermissionsToRoleOutcome Outcome, string? Error)> AssignAsync(
        Guid actorUserId, Guid roleId, IReadOnlyList<string> permissionNames, CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty)
        {
            return (AssignPermissionsToRoleOutcome.Unauthorized, null);
        }

        if (!await roleManagement.CanManageRoleAsync(actorUserId, roleId))
        {
            return (AssignPermissionsToRoleOutcome.Forbidden, "Insufficient privileges to manage this role");
        }

        try
        {
            await roleManagement.AssignPermissionsToRoleAsync(roleId, permissionNames.ToList());
            return (AssignPermissionsToRoleOutcome.Success, null);
        }
        catch (Exception ex)
        {
            return (AssignPermissionsToRoleOutcome.Invalid, $"Failed to assign permissions: {ex.Message}");
        }
    }
}
