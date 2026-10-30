namespace PolyBucket.Api.Features.ACL.AssignPermissionsToRole.Domain;

public enum AssignPermissionsToRoleOutcome { Success, Unauthorized, Forbidden, Invalid }

public interface IAssignPermissionsToRoleService
{
    Task<(AssignPermissionsToRoleOutcome Outcome, string? Error)> AssignAsync(
        Guid actorUserId, Guid roleId, IReadOnlyList<string> permissionNames, CancellationToken cancellationToken = default);
}
