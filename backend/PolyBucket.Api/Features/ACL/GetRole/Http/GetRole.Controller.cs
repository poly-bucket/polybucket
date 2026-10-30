using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.GetRole.Domain;

namespace PolyBucket.Api.Features.ACL.GetRole.Http;

[Authorize]
[ApiController]
[Route("api/admin/roles")]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_ROLES)]
public class GetRoleController(IGetRoleService service) : ControllerBase
{
    /// <summary>
    /// Get role by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<RoleDto>> GetRole(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetRoleAsync(id, cancellationToken);
        return dto == null ? NotFound("Role not found") : Ok(dto);
    }
}
