namespace PolyBucket.Api.Features.ACL.RemovePermissionsFromRole.Domain;

public enum RemovePermissionsFromRoleOutcome { Success, Unauthorized, Forbidden, Invalid }

public interface IRemovePermissionsFromRoleService
{
    Task<(RemovePermissionsFromRoleOutcome Outcome, string? Error)> RemoveAsync(
        Guid actorUserId, Guid roleId, IReadOnlyList<string> permissionNames, CancellationToken cancellationToken = default);
}
