using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.CanUserModerateUser.Domain;
using PolyBucket.Api.Features.ACL.CanUserModerateUser.Http;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.ACL.CanUserModerateUser.Http;

[Authorize]
[ApiController]
[Route("api/admin/user-permissions")]
public class CanUserModerateUserController(ICanUserModerateUserService service) : ControllerBase
{
    /// <summary>
    /// Check whether a user can moderate another user.
    /// </summary>
    [HttpGet("{moderatorId}/can-moderate/{targetUserId}")]
    [RequirePermission(PermissionConstants.ADMIN_VIEW_USER_DETAILS)]
    [ProducesResponseType(typeof(ModerationCheckResponseDto), 200)]
    public async Task<ActionResult<ModerationCheckResponseDto>> CanUserModerateUser(
        Guid moderatorId, Guid targetUserId, CancellationToken cancellationToken) =>
        Ok(await service.CanUserModerateUserAsync(moderatorId, targetUserId, cancellationToken));
}
