namespace PolyBucket.Api.Features.ACL.AssignUserRole.Domain;

public enum AssignUserRoleOutcome { Success, Unauthorized, Forbidden, Invalid }

public interface IAssignUserRoleService
{
    Task<(AssignUserRoleOutcome Outcome, string? Error)> AssignUserRoleAsync(
        Guid actorUserId, Guid targetUserId, Guid roleId, CancellationToken cancellationToken = default);
}
