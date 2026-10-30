using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.AssignUserRole.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.ACL.AssignUserRole.Http;

[Authorize]
[ApiController]
[Route("api/admin/user-permissions")]
public class AssignUserRoleController(IAssignUserRoleService service) : ControllerBase
{
    /// <summary>
    /// Assign a role to a user.
    /// </summary>
    [HttpPost("{userId}/role")]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_USERS)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> AssignUserRole(Guid userId, [FromBody] AssignUserRoleRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var actorUserId))
        {
            return Unauthorized();
        }

        var (outcome, error) = await service.AssignUserRoleAsync(actorUserId, userId, request.RoleId, cancellationToken);
        return outcome switch
        {
            AssignUserRoleOutcome.Unauthorized => Unauthorized(),
            AssignUserRoleOutcome.Forbidden => Forbid(error),
            AssignUserRoleOutcome.Invalid => BadRequest(error),
            _ => Ok(new { message = "Role assigned successfully" })
        };
    }
}
