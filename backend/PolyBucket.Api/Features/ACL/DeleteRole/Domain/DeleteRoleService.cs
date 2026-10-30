using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.DeleteRole.Domain;

public class DeleteRoleService(IRoleManagementService roleManagement) : IDeleteRoleService
{
    public async Task<(DeleteRoleOutcome Outcome, string? Error)> DeleteRoleAsync(
        Guid actorUserId, Guid roleId, CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty)
        {
            return (DeleteRoleOutcome.Unauthorized, null);
        }

        if (!await roleManagement.CanDeleteRoleAsync(actorUserId, roleId))
        {
            return (DeleteRoleOutcome.Forbidden, "Insufficient privileges to delete this role");
        }

        try
        {
            var success = await roleManagement.DeleteRoleAsync(roleId);
            return success
                ? (DeleteRoleOutcome.Success, null)
                : (DeleteRoleOutcome.NotFound, "Role not found or cannot be deleted");
        }
        catch (InvalidOperationException ex)
        {
            return (DeleteRoleOutcome.Invalid, ex.Message);
        }
        catch (Exception ex)
        {
            return (DeleteRoleOutcome.Invalid, $"Failed to delete role: {ex.Message}");
        }
    }
}
