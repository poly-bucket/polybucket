using PolyBucket.Api.Features.ACL.GetUserPermissions.Http;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.GetUserPermissions.Domain;

public class GetUserPermissionsService(IPermissionService permissionService) : IGetUserPermissionsService
{
    public Task<UserPermissionsResponseDto?> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        UserPermissionsResponseBuilder.BuildAsync(userId, permissionService);
}
