using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.GetAllRoles.Domain;
using PolyBucket.Api.Features.ACL.GetAllRoles.Http;

namespace PolyBucket.Api.Features.ACL.GetAllRoles.Http;

[Authorize]
[ApiController]
[Route("api/admin/roles")]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_ROLES)]
public class GetAllRolesController(IGetAllRolesService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedRolesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedRolesResponse>> GetAllRoles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "priority",
        [FromQuery] bool sortDescending = true) =>
        Ok(await service.GetAllRolesAsync(page, pageSize, searchTerm, sortBy, sortDescending));
}
