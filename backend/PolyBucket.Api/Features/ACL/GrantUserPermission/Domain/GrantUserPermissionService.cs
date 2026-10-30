using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.GrantUserPermission.Domain;

public class GrantUserPermissionService(IPermissionService permissionService) : IGrantUserPermissionService
{
    public async Task<(GrantUserPermissionOutcome Outcome, string? Error)> GrantAsync(
        Guid actorUserId, Guid targetUserId, string permission, string? reason, DateTime? expiresAt, CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty)
        {
            return (GrantUserPermissionOutcome.Unauthorized, null);
        }

        if (permission.StartsWith("admin.", StringComparison.Ordinal) && !await permissionService.IsAdminAsync(actorUserId))
        {
            return (GrantUserPermissionOutcome.Forbidden, "Only administrators can grant admin permissions");
        }

        try
        {
            var success = await permissionService.GrantUserPermissionAsync(targetUserId, permission, actorUserId, reason, expiresAt);
            return success
                ? (GrantUserPermissionOutcome.Success, null)
                : (GrantUserPermissionOutcome.Invalid, "Failed to grant permission");
        }
        catch (Exception ex)
        {
            return (GrantUserPermissionOutcome.Invalid, $"Failed to grant permission: {ex.Message}");
        }
    }
}
