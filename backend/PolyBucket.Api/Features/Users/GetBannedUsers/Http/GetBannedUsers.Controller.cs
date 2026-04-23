using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Users.GetBannedUsers.Domain;
using PolyBucket.Api.Features.Users.GetBannedUsers.Http;

namespace PolyBucket.Api.Features.Users.GetBannedUsers.Http;

[ApiController]
[Route("api/admin/users")]
[Authorize]
[RequirePermission(PermissionConstants.ADMIN_BAN_USERS)]
public class GetBannedUsersController(IGetBannedUsersService getBannedUsersService) : ControllerBase
{
    /// <summary>
    /// Get all banned users
    /// </summary>
    [HttpGet("banned")]
    [ProducesResponseType(typeof(BannedUsersListResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetBannedUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Invalid pagination parameters");
        }

        var response = await getBannedUsersService.GetBannedUsersAsync(page, pageSize, cancellationToken);
        return Ok(response);
    }
}
