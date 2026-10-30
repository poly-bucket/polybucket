using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.GetAllRolesUnpaginated.Domain;

namespace PolyBucket.Api.Features.ACL.GetAllRolesUnpaginated.Http;

[Authorize]
[ApiController]
[Route("api/admin/roles")]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_ROLES)]
public class GetAllRolesUnpaginatedController(IGetAllRolesUnpaginatedService service) : ControllerBase
{
    /// <summary>
    /// Get all roles without pagination.
    /// </summary>
    [HttpGet("unpaginated")]
    [ProducesResponseType(typeof(List<RoleDto>), 200)]
    public async Task<ActionResult<List<RoleDto>>> GetAllRolesUnpaginated(CancellationToken cancellationToken) =>
        Ok(await service.GetAllRolesUnpaginatedAsync(cancellationToken));
}
