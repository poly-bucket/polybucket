using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Users.UnbanUser.Domain;

namespace PolyBucket.Api.Features.Users.UnbanUser.Http;

[ApiController]
[Route("api/admin/users")]
[Authorize]
[RequirePermission(PermissionConstants.ADMIN_BAN_USERS)]
public class UnbanUserController(IUnbanUserService unbanUserService) : ControllerBase
{
    /// <summary>
    /// Unban a user
    /// </summary>
    [HttpPost("{userId}/unban")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UnbanUser([FromRoute] Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await unbanUserService.UnbanUserAsync(userId, cancellationToken);
            return Ok(new { message = "User unbanned successfully" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "User not found")
        {
            return NotFound("User not found");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
