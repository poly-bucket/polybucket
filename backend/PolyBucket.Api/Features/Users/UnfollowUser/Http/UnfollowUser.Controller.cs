using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.UnfollowUser.Domain;

namespace PolyBucket.Api.Features.Users.UnfollowUser.Http;

[ApiController]
[Route("api/users")]
[Authorize]
public class UnfollowUserController(IUnfollowUserService unfollowUserService) : ControllerBase
{
    /// <summary>
    /// Unfollow a user. Idempotent when not currently following.
    /// </summary>
    [HttpDelete("{userId}/follow")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> UnfollowUser([FromRoute] Guid userId, CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var followerUserId))
        {
            return Unauthorized();
        }

        await unfollowUserService.UnfollowAsync(followerUserId, userId, cancellationToken);
        return Ok(new { message = "Unfollowed user" });
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        userId = default;
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out userId);
    }
}
