using PolyBucket.Api.Features.ACL.GetAllRoles.Http;

namespace PolyBucket.Api.Features.ACL.GetRole.Domain;

public interface IGetRoleService
{
    Task<RoleDto?> GetRoleAsync(Guid id, CancellationToken cancellationToken = default);
}
