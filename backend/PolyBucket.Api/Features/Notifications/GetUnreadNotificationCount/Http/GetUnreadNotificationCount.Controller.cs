using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Notifications.GetUnreadNotificationCount.Domain;

namespace PolyBucket.Api.Features.Notifications.GetUnreadNotificationCount.Http;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class GetUnreadNotificationCountController(IGetUnreadNotificationCountService service) : ControllerBase
{
    /// <summary>
    /// Gets how many unread notifications the signed-in user has.
    /// </summary>
    /// <remarks>Cheap enough to poll; the header bell calls this periodically.</remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The unread count.</response>
    /// <response code="401">The caller is not signed in.</response>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadNotificationCountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindUserIdClaim(), out var userId))
        {
            return Unauthorized();
        }

        Response.Headers.CacheControl = "no-store";
        return Ok(new UnreadNotificationCountResponse(await service.GetUnreadCountAsync(userId, cancellationToken)));
    }
}

public record UnreadNotificationCountResponse(int Count);
