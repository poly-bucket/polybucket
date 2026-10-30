using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.RemovePermissionsFromRole.Domain;

namespace PolyBucket.Api.Features.ACL.RemovePermissionsFromRole.Http;

[Authorize]
[ApiController]
[Route("api/admin/roles")]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_ROLES)]
public class RemovePermissionsFromRoleController(IRemovePermissionsFromRoleService service) : ControllerBase
{
    /// <summary>
    /// Remove permissions from a role.
    /// </summary>
    [HttpDelete("{id}/permissions")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> RemovePermissionsFromRole(
        Guid id, [FromBody] RemovePermissionsFromRoleRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Unauthorized();
        }

        var (outcome, error) = await service.RemoveAsync(userId, id, request.PermissionNames, cancellationToken);
        return outcome switch
        {
            RemovePermissionsFromRoleOutcome.Unauthorized => Unauthorized(),
            RemovePermissionsFromRoleOutcome.Forbidden => Forbid(error),
            RemovePermissionsFromRoleOutcome.Invalid => BadRequest(error),
            _ => Ok(new { message = "Permissions removed successfully" })
        };
    }
}
