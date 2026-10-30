using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.GetFollowerHistory.Domain;

namespace PolyBucket.Api.Features.Users.GetFollowerHistory.Http;

[ApiController]
[Route("api/users")]
[Authorize]
public class GetFollowerHistoryController(IGetFollowerHistoryService getFollowerHistoryService) : ControllerBase
{
    /// <summary>
    /// Get cumulative follower counts over time for the authenticated profile owner.
    /// </summary>
    [HttpGet("{userId}/followers/history")]
    [ProducesResponseType(200, Type = typeof(GetFollowerHistoryResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<GetFollowerHistoryResult>> GetFollowerHistory(
        [FromRoute] Guid userId,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var requestingUserId))
        {
            return Unauthorized();
        }

        var query = new GetFollowerHistoryQuery
        {
            UserId = userId,
            Days = days,
            RequestingUserId = requestingUserId
        };

        var (outcome, result) = await getFollowerHistoryService.GetHistoryAsync(query, cancellationToken);
        return outcome switch
        {
            GetFollowerHistoryOutcome.Success => Ok(result),
            GetFollowerHistoryOutcome.InvalidDays => BadRequest(new { message = "Days must be 7, 30, or 90" }),
            GetFollowerHistoryOutcome.UserNotFound => NotFound(new { message = "User not found" }),
            GetFollowerHistoryOutcome.Forbidden => StatusCode(403, new { message = "You can only view your own follower history" }),
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
