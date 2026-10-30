using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.AssignPermissionsToRole.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.ACL.AssignPermissionsToRole.Http;

[Authorize]
[ApiController]
[Route("api/admin/roles")]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_ROLES)]
public class AssignPermissionsToRoleController(IAssignPermissionsToRoleService service) : ControllerBase
{
    /// <summary>
    /// Assign permissions to a role.
    /// </summary>
    [HttpPost("{id}/permissions")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> AssignPermissionsToRole(
        Guid id, [FromBody] AssignPermissionsToRoleRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Unauthorized();
        }

        var (outcome, error) = await service.AssignAsync(userId, id, request.PermissionNames, cancellationToken);
        return outcome switch
        {
            AssignPermissionsToRoleOutcome.Unauthorized => Unauthorized(),
            AssignPermissionsToRoleOutcome.Forbidden => Forbid(error),
            AssignPermissionsToRoleOutcome.Invalid => BadRequest(error),
            _ => Ok(new { message = "Permissions assigned successfully" })
        };
    }
}
