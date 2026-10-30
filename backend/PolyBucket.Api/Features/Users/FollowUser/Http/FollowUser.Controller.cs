using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.FollowUser.Domain;

namespace PolyBucket.Api.Features.Users.FollowUser.Http;

[ApiController]
[Route("api/users")]
[Authorize]
public class FollowUserController(IFollowUserService followUserService) : ControllerBase
{
    /// <summary>
    /// Follow a user. The target profile must be public and not banned.
    /// </summary>
    [HttpPost("{userId}/follow")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> FollowUser([FromRoute] Guid userId, CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var followerUserId))
        {
            return Unauthorized();
        }

        var outcome = await followUserService.FollowAsync(followerUserId, userId, cancellationToken);
        return outcome switch
        {
            FollowUserOutcome.Success => Ok(new { message = "Followed user" }),
            FollowUserOutcome.AlreadyFollowing => Ok(new { message = "Already following user" }),
            FollowUserOutcome.SelfFollow => BadRequest(new { message = "You cannot follow yourself" }),
            FollowUserOutcome.TargetNotFound => NotFound(new { message = "User not found" }),
            FollowUserOutcome.TargetPrivate => StatusCode(403, new { message = "This profile is private" }),
            FollowUserOutcome.TargetBanned => StatusCode(403, new { message = "This user cannot be followed" }),
            _ => StatusCode(500, new { message = "An error occurred" })
        };
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        userId = default;
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out userId);
    }
}
