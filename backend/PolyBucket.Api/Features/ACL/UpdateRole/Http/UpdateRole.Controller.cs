using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.UpdateRole.Domain;

namespace PolyBucket.Api.Features.ACL.UpdateRole.Http;

[Authorize]
[ApiController]
[Route("api/admin/roles")]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_ROLES)]
public class UpdateRoleController(IUpdateRoleService service) : ControllerBase
{
    /// <summary>
    /// Update an existing role.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoleDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<RoleDto>> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Unauthorized();
        }

        var (outcome, role, error) = await service.UpdateRoleAsync(userId, id, request, cancellationToken);
        return outcome switch
        {
            UpdateRoleOutcome.Unauthorized => Unauthorized(),
            UpdateRoleOutcome.Forbidden => Forbid(error),
            UpdateRoleOutcome.NotFound => NotFound(error),
            UpdateRoleOutcome.Invalid => BadRequest(error),
            _ => Ok(role)
        };
    }
}
