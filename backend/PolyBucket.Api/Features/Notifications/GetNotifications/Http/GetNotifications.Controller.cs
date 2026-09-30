using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Api.Features.Notifications.GetNotifications.Domain;

namespace PolyBucket.Api.Features.Notifications.GetNotifications.Http;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class GetNotificationsController(IGetNotificationsService service) : ControllerBase
{
    /// <summary>
    /// Lists the signed-in user's notifications, newest first.
    /// </summary>
    /// <param name="page">Page number, starting at 1.</param>
    /// <param name="pageSize">Notifications per page, 1 to 50.</param>
    /// <param name="unreadOnly">Only return unread notifications.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">A page of notifications with the unread total.</response>
    /// <response code="400">The paging values are invalid.</response>
    /// <response code="401">The caller is not signed in.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetNotificationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindUserIdClaim(), out var userId))
        {
            return Unauthorized();
        }

        if (page < 1 || pageSize < 1 || pageSize > NotificationLimits.MaxPageSize)
        {
            return BadRequest(new { message = $"Page must be at least 1 and page size between 1 and {NotificationLimits.MaxPageSize}" });
        }

        return Ok(await service.GetNotificationsAsync(userId, unreadOnly, page, pageSize, cancellationToken));
    }
}
