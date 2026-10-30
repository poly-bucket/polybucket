using PolyBucket.Api.Features.ACL.CreateRole.Http;
using PolyBucket.Api.Features.ACL.GetAllRoles.Http;

namespace PolyBucket.Api.Features.ACL.CreateRole.Domain;

public enum CreateRoleOutcome { Success, Unauthorized, Invalid }

public interface ICreateRoleService
{
    Task<(CreateRoleOutcome Outcome, RoleDto? Role, string? Error)> CreateRoleAsync(
        Guid actorUserId, CreateRoleRequest request, CancellationToken cancellationToken = default);
}
