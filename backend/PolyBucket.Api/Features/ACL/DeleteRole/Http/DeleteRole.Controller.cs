using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.DeleteRole.Domain;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.ACL.DeleteRole.Http;

[Authorize]
[ApiController]
[Route("api/admin/roles")]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_ROLES)]
public class DeleteRoleController(IDeleteRoleService service) : ControllerBase
{
    /// <summary>
    /// Delete a role.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Unauthorized();
        }

        var (outcome, error) = await service.DeleteRoleAsync(userId, id, cancellationToken);
        return outcome switch
        {
            DeleteRoleOutcome.Unauthorized => Unauthorized(),
            DeleteRoleOutcome.Forbidden => Forbid(error),
            DeleteRoleOutcome.NotFound => NotFound(error),
            DeleteRoleOutcome.Invalid => BadRequest(error),
            _ => NoContent()
        };
    }
}
