using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.GetUserFollowers.Domain;
using PolyBucket.Api.Features.Users.GetUserFollowing.Domain;

namespace PolyBucket.Api.Features.Users.GetUserFollowing.Http;

[ApiController]
[Route("api/users")]
public class GetUserFollowingController(
    IGetUserFollowingService getUserFollowingService,
    ILogger<GetUserFollowingController> logger) : ControllerBase
{
    /// <summary>
    /// Get users that the specified user follows.
    /// </summary>
    [HttpGet("{userId}/following")]
    [ProducesResponseType(200, Type = typeof(GetUserFollowListResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<GetUserFollowListResult>> GetFollowingByUserId(
        [FromRoute] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await GetFollowingInternal(
            new GetUserFollowListQuery { UserId = userId, Page = page, PageSize = pageSize },
            userId,
            null,
            cancellationToken);
    }

    /// <summary>
    /// Get users that the specified profile username follows.
    /// </summary>
    [HttpGet("profile/{username}/following")]
    [ProducesResponseType(200, Type = typeof(GetUserFollowListResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<GetUserFollowListResult>> GetFollowingByUsername(
        [FromRoute] string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await GetFollowingInternal(
            new GetUserFollowListQuery { Username = username, Page = page, PageSize = pageSize },
            null,
            username,
            cancellationToken);
    }

    private async Task<ActionResult<GetUserFollowListResult>> GetFollowingInternal(
        GetUserFollowListQuery query,
        Guid? userIdForLog,
        string? usernameForLog,
        CancellationToken cancellationToken)
    {
        if (query.Page < 1)
        {
            return BadRequest("Page number must be greater than 0");
        }

        if (query.PageSize < 1 || query.PageSize > 100)
        {
            return BadRequest("Page size must be between 1 and 100");
        }

        PopulateRequesterContext(query);

        try
        {
            var (outcome, result) = await getUserFollowingService.GetFollowingAsync(query, cancellationToken);
            return outcome switch
            {
                GetUserFollowListOutcome.Success => Ok(result),
                GetUserFollowListOutcome.UserNotFound => NotFound(new { message = "User not found" }),
                GetUserFollowListOutcome.ProfilePrivate => NotFound(new { message = "User not found" }),
                _ => StatusCode(500, new { message = "An error occurred" })
            };
        }
        catch (Exception ex)
        {
            if (userIdForLog.HasValue)
            {
                logger.LogError(ex, "Error retrieving following for user {UserId}", userIdForLog);
            }
            else
            {
                logger.LogError(ex, "Error retrieving following for username {Username}", usernameForLog);
            }

            return StatusCode(500, new { message = "An error occurred while retrieving following" });
        }
    }

    private void PopulateRequesterContext(GetUserFollowListQuery query)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            query.RequestingUserId = userId;
        }

        query.IsRequestingUserAdmin = User.IsInRole("Admin");
    }
}
