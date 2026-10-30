using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.RevokeUserPermission.Domain;

public class RevokeUserPermissionService(IPermissionService permissionService) : IRevokeUserPermissionService
{
    public async Task<(RevokeUserPermissionOutcome Outcome, string? Error)> RevokeAsync(
        Guid actorUserId, Guid targetUserId, string permission, CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty)
        {
            return (RevokeUserPermissionOutcome.Unauthorized, null);
        }

        if (permission.StartsWith("admin.", StringComparison.Ordinal) && !await permissionService.IsAdminAsync(actorUserId))
        {
            return (RevokeUserPermissionOutcome.Forbidden, "Only administrators can revoke admin permissions");
        }

        try
        {
            var success = await permissionService.RevokeUserPermissionAsync(targetUserId, permission);
            return success
                ? (RevokeUserPermissionOutcome.Success, null)
                : (RevokeUserPermissionOutcome.Invalid, "Failed to revoke permission");
        }
        catch (Exception ex)
        {
            return (RevokeUserPermissionOutcome.Invalid, $"Failed to revoke permission: {ex.Message}");
        }
    }
}
