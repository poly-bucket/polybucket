using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.RemovePermissionsFromRole.Domain;

public class RemovePermissionsFromRoleService(IRoleManagementService roleManagement) : IRemovePermissionsFromRoleService
{
    public async Task<(RemovePermissionsFromRoleOutcome Outcome, string? Error)> RemoveAsync(
        Guid actorUserId, Guid roleId, IReadOnlyList<string> permissionNames, CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty)
        {
            return (RemovePermissionsFromRoleOutcome.Unauthorized, null);
        }

        if (!await roleManagement.CanManageRoleAsync(actorUserId, roleId))
        {
            return (RemovePermissionsFromRoleOutcome.Forbidden, "Insufficient privileges to manage this role");
        }

        try
        {
            await roleManagement.RemovePermissionsFromRoleAsync(roleId, permissionNames.ToList());
            return (RemovePermissionsFromRoleOutcome.Success, null);
        }
        catch (Exception ex)
        {
            return (RemovePermissionsFromRoleOutcome.Invalid, $"Failed to remove permissions: {ex.Message}");
        }
    }
}
