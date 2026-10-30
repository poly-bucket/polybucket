using PolyBucket.Api.Features.ACL.GetUserPermissions.Http;

namespace PolyBucket.Api.Features.ACL.GetUserPermissions.Domain;

public interface IGetUserPermissionsService
{
    Task<UserPermissionsResponseDto?> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
