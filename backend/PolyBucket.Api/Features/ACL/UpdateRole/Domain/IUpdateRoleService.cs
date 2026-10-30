using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.UpdateRole.Http;

namespace PolyBucket.Api.Features.ACL.UpdateRole.Domain;

public enum UpdateRoleOutcome { Success, Unauthorized, Forbidden, NotFound, Invalid }

public interface IUpdateRoleService
{
    Task<(UpdateRoleOutcome Outcome, RoleDto? Role, string? Error)> UpdateRoleAsync(
        Guid actorUserId, Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default);
}
