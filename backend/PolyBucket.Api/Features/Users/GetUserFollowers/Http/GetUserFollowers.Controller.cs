using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.GetUserFollowers.Domain;

namespace PolyBucket.Api.Features.Users.GetUserFollowers.Http;

[ApiController]
[Route("api/users")]
public class GetUserFollowersController(
    IGetUserFollowersService getUserFollowersService,
    ILogger<GetUserFollowersController> logger) : ControllerBase
{
    /// <summary>
    /// Get users who follow the specified user.
    /// </summary>
    [HttpGet("{userId}/followers")]
    [ProducesResponseType(200, Type = typeof(GetUserFollowListResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<GetUserFollowListResult>> GetFollowersByUserId(
        [FromRoute] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await GetFollowersInternal(
            new GetUserFollowListQuery { UserId = userId, Page = page, PageSize = pageSize },
            userId,
            null,
            cancellationToken);
    }

    /// <summary>
    /// Get users who follow the specified profile username.
    /// </summary>
    [HttpGet("profile/{username}/followers")]
    [ProducesResponseType(200, Type = typeof(GetUserFollowListResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<GetUserFollowListResult>> GetFollowersByUsername(
        [FromRoute] string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await GetFollowersInternal(
            new GetUserFollowListQuery { Username = username, Page = page, PageSize = pageSize },
            null,
            username,
            cancellationToken);
    }

    private async Task<ActionResult<GetUserFollowListResult>> GetFollowersInternal(
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
            var (outcome, result) = await getUserFollowersService.GetFollowersAsync(query, cancellationToken);
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
                logger.LogError(ex, "Error retrieving followers for user {UserId}", userIdForLog);
            }
            else
            {
                logger.LogError(ex, "Error retrieving followers for username {Username}", usernameForLog);
            }

            return StatusCode(500, new { message = "An error occurred while retrieving followers" });
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
