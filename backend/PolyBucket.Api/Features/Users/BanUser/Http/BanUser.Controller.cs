using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Users.BanUser.Domain;

namespace PolyBucket.Api.Features.Users.BanUser.Http;

[ApiController]
[Route("api/admin/users")]
[Authorize]
[RequirePermission(PermissionConstants.ADMIN_BAN_USERS)]
public class BanUserController(IBanUserService banUserService) : ControllerBase
{
    /// <summary>
    /// Ban a user
    /// </summary>
    /// <param name="userId">User ID to ban</param>
    /// <param name="request">Ban details</param>
    [HttpPost("{userId}/ban")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> BanUser([FromRoute] Guid userId, [FromBody] BanUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            await banUserService.BanUserAsync(userId, currentUserId, request.Reason, request.ExpiresAt, cancellationToken);
            return Ok(new { message = "User banned successfully" });
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
