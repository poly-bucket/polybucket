using PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Http;

namespace PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Domain;

public interface IGetRolePermissionsCatalogService
{
    Task<IReadOnlyList<PermissionCatalogItemDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
}
