using PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Http;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Domain;

public class GetRolePermissionsCatalogService(IPermissionService permissionService) : IGetRolePermissionsCatalogService
{
    public async Task<IReadOnlyList<PermissionCatalogItemDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await permissionService.GetAllPermissionsAsync();
        return permissions
            .Select(p => new PermissionCatalogItemDto
            {
                Name = p.Name,
                Category = RolePermissionCatalogFormatter.GetCategory(p.Name),
                Description = RolePermissionCatalogFormatter.GetDescription(p.Name)
            })
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToList();
    }
}
