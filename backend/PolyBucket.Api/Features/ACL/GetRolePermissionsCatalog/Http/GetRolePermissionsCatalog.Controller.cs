using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Domain;
using PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Http;

namespace PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Http;

[Authorize]
[ApiController]
[Route("api/admin/roles")]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_ROLES)]
public class GetRolePermissionsCatalogController(IGetRolePermissionsCatalogService service) : ControllerBase
{
    /// <summary>
    /// Get all available permissions for role assignment.
    /// </summary>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(List<PermissionCatalogItemDto>), 200)]
    public async Task<ActionResult<List<PermissionCatalogItemDto>>> GetAllPermissions(CancellationToken cancellationToken) =>
        Ok((await service.GetAllPermissionsAsync(cancellationToken)).ToList());
}
