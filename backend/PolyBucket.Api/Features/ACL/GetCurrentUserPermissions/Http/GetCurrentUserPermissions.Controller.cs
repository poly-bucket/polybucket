using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.GetCurrentUserPermissions.Domain;
using PolyBucket.Api.Features.ACL.GetCurrentUserPermissions.Http;

namespace PolyBucket.Api.Features.ACL.GetCurrentUserPermissions.Http;

[Authorize]
[ApiController]
[Route("api/admin/user-permissions")]
public class GetCurrentUserPermissionsController(IGetCurrentUserPermissionsService service) : ControllerBase
{
    /// <summary>
    /// Get the current user's permissions.
    /// </summary>
    [HttpGet("me/permissions")]
    [ProducesResponseType(typeof(UserPermissionsResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserPermissionsResponseDto>> GetCurrentUserPermissions(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Unauthorized();
        }

        var dto = await service.GetCurrentUserPermissionsAsync(userId, cancellationToken);
        return dto == null ? NotFound("User not found") : Ok(dto);
    }
}
