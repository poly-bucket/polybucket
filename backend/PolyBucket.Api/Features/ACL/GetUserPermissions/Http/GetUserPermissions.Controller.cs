using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.GetUserPermissions.Domain;
using PolyBucket.Api.Features.ACL.GetUserPermissions.Http;

namespace PolyBucket.Api.Features.ACL.GetUserPermissions.Http;

[Authorize]
[ApiController]
[Route("api/admin/user-permissions")]
public class GetUserPermissionsController(IGetUserPermissionsService service) : ControllerBase
{
    /// <summary>
    /// Get a user's effective permissions.
    /// </summary>
    [HttpGet("{userId}/permissions")]
    [RequirePermission(PermissionConstants.ADMIN_VIEW_USER_DETAILS)]
    [ProducesResponseType(typeof(UserPermissionsResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserPermissionsResponseDto>> GetUserPermissions(Guid userId, CancellationToken cancellationToken)
    {
        var dto = await service.GetUserPermissionsAsync(userId, cancellationToken);
        return dto == null ? NotFound("User not found") : Ok(dto);
    }
}
