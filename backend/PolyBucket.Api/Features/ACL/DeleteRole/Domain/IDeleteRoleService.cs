namespace PolyBucket.Api.Features.ACL.DeleteRole.Domain;

public enum DeleteRoleOutcome { Success, Unauthorized, Forbidden, NotFound, Invalid }

public interface IDeleteRoleService
{
    Task<(DeleteRoleOutcome Outcome, string? Error)> DeleteRoleAsync(
        Guid actorUserId, Guid roleId, CancellationToken cancellationToken = default);
}
