using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.GetRolePermissionsCatalog.Http;
using PolyBucket.Api.Features.ACL.GetUserPermissionCatalog.Domain;

namespace PolyBucket.Api.Features.ACL.GetUserPermissionCatalog.Http;

[Authorize]
[ApiController]
[Route("api/admin/user-permissions")]
public class GetUserPermissionCatalogController(IGetUserPermissionCatalogService service) : ControllerBase
{
    /// <summary>
    /// Get all assignable permissions from constants.
    /// </summary>
    [HttpGet("permissions")]
    [RequirePermission(PermissionConstants.ADMIN_VIEW_USER_DETAILS)]
    [ProducesResponseType(typeof(List<PermissionCatalogItemDto>), 200)]
    public ActionResult<List<PermissionCatalogItemDto>> GetAllPermissions() =>
        Ok(service.GetAllPermissions().ToList());
}
