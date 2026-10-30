namespace PolyBucket.Api.Features.ACL.GrantUserPermission.Domain;

public enum GrantUserPermissionOutcome { Success, Unauthorized, Forbidden, Invalid }

public interface IGrantUserPermissionService
{
    Task<(GrantUserPermissionOutcome Outcome, string? Error)> GrantAsync(
        Guid actorUserId, Guid targetUserId, string permission, string? reason, DateTime? expiresAt, CancellationToken cancellationToken = default);
}
