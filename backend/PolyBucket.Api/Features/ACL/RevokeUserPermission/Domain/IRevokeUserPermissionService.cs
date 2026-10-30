namespace PolyBucket.Api.Features.ACL.RevokeUserPermission.Domain;

public enum RevokeUserPermissionOutcome { Success, Unauthorized, Forbidden, Invalid }

public interface IRevokeUserPermissionService
{
    Task<(RevokeUserPermissionOutcome Outcome, string? Error)> RevokeAsync(
        Guid actorUserId, Guid targetUserId, string permission, CancellationToken cancellationToken = default);
}
