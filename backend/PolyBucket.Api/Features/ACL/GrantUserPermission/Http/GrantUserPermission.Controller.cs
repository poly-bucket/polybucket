using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.GrantUserPermission.Domain;

namespace PolyBucket.Api.Features.ACL.GrantUserPermission.Http;

[Authorize]
[ApiController]
[Route("api/admin/user-permissions")]
public class GrantUserPermissionController(IGrantUserPermissionService service) : ControllerBase
{
    /// <summary>
    /// Grant a permission override to a user.
    /// </summary>
    [HttpPost("{userId}/permissions/grant")]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_PERMISSIONS)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> GrantUserPermission(Guid userId, [FromBody] GrantUserPermissionRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var actorUserId))
        {
            return Unauthorized();
        }

        var (outcome, error) = await service.GrantAsync(actorUserId, userId, request.Permission, request.Reason, request.ExpiresAt, cancellationToken);
        return outcome switch
        {
            GrantUserPermissionOutcome.Unauthorized => Unauthorized(),
            GrantUserPermissionOutcome.Forbidden => Forbid(error),
            GrantUserPermissionOutcome.Invalid => BadRequest(error),
            _ => Ok(new { message = "Permission granted successfully" })
        };
    }
}
