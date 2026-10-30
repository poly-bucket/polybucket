using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.RevokeUserPermission.Domain;

namespace PolyBucket.Api.Features.ACL.RevokeUserPermission.Http;

[Authorize]
[ApiController]
[Route("api/admin/user-permissions")]
public class RevokeUserPermissionController(IRevokeUserPermissionService service) : ControllerBase
{
    /// <summary>
    /// Revoke a permission override from a user.
    /// </summary>
    [HttpDelete("{userId}/permissions/{permission}")]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_PERMISSIONS)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> RevokeUserPermission(Guid userId, string permission, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var actorUserId))
        {
            return Unauthorized();
        }

        var (outcome, error) = await service.RevokeAsync(actorUserId, userId, permission, cancellationToken);
        return outcome switch
        {
            RevokeUserPermissionOutcome.Unauthorized => Unauthorized(),
            RevokeUserPermissionOutcome.Forbidden => Forbid(error),
            RevokeUserPermissionOutcome.Invalid => BadRequest(error),
            _ => Ok(new { message = "Permission revoked successfully" })
        };
    }
}
