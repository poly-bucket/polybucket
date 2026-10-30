using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.AssignUserRole.Domain;

public class AssignUserRoleService(IPermissionService permissionService) : IAssignUserRoleService
{
    public async Task<(AssignUserRoleOutcome Outcome, string? Error)> AssignUserRoleAsync(
        Guid actorUserId, Guid targetUserId, Guid roleId, CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty)
        {
            return (AssignUserRoleOutcome.Unauthorized, null);
        }

        if (!await permissionService.CanManageRoleAsync(actorUserId, roleId))
        {
            return (AssignUserRoleOutcome.Forbidden, "Insufficient privileges to assign this role");
        }

        var targetRole = (await permissionService.GetAllRolesAsync()).FirstOrDefault(r => r.Id == roleId);
        if (targetRole?.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true
            && !await permissionService.IsAdminAsync(actorUserId))
        {
            return (AssignUserRoleOutcome.Forbidden, "Only administrators can assign the admin role");
        }

        try
        {
            var success = await permissionService.AssignRoleAsync(targetUserId, roleId, actorUserId);
            return success
                ? (AssignUserRoleOutcome.Success, null)
                : (AssignUserRoleOutcome.Invalid, "Failed to assign role");
        }
        catch (Exception ex)
        {
            return (AssignUserRoleOutcome.Invalid, $"Failed to assign role: {ex.Message}");
        }
    }
}
