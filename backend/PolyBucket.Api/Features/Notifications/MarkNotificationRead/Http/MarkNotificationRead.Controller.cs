using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Notifications.MarkNotificationRead.Domain;

namespace PolyBucket.Api.Features.Notifications.MarkNotificationRead.Http;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class MarkNotificationReadController(IMarkNotificationReadService service) : ControllerBase
{
    /// <summary>
    /// Marks one of the signed-in user's notifications as read.
    /// </summary>
    /// <remarks>Marking an already-read notification succeeds without changing its read time.</remarks>
    /// <param name="notificationId">The notification id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">The notification is read.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="404">The notification does not exist or belongs to someone else.</response>
    [HttpPost("{notificationId:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid notificationId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindUserIdClaim(), out var userId))
        {
            return Unauthorized();
        }

        return await service.MarkReadAsync(userId, notificationId, cancellationToken) ? NoContent() : NotFound();
    }
}
