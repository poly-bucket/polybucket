using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Notifications.MarkAllNotificationsRead.Domain;

namespace PolyBucket.Api.Features.Notifications.MarkAllNotificationsRead.Http;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class MarkAllNotificationsReadController(IMarkAllNotificationsReadService service) : ControllerBase
{
    /// <summary>
    /// Marks all of the signed-in user's unread notifications as read.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">How many notifications were marked read.</response>
    /// <response code="401">The caller is not signed in.</response>
    [HttpPost("read-all")]
    [ProducesResponseType(typeof(MarkAllNotificationsReadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindUserIdClaim(), out var userId))
        {
            return Unauthorized();
        }

        return Ok(new MarkAllNotificationsReadResponse(await service.MarkAllReadAsync(userId, cancellationToken)));
    }
}

public record MarkAllNotificationsReadResponse(int Updated);
