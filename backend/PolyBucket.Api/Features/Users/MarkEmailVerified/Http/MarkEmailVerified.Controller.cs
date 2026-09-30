using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Users.MarkEmailVerified.Domain;

namespace PolyBucket.Api.Features.Users.MarkEmailVerified.Http;

[ApiController]
[Route("api/admin/users")]
[Authorize]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_USERS)]
public class MarkEmailVerifiedController(IMarkEmailVerifiedService markEmailVerifiedService) : ControllerBase
{
    /// <summary>
    /// Marks a user's email address as verified without sending an email.
    /// </summary>
    /// <remarks>
    /// Use this when email delivery is unavailable or the address was confirmed another way.
    /// The action is recorded in the user audit log. Calling it for an already verified user is a no-op.
    /// </remarks>
    /// <param name="userId">The user to mark as verified.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The user is verified; the response contains the verification time.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="403">The caller cannot manage users.</response>
    /// <response code="404">The user does not exist.</response>
    [HttpPost("{userId}/verify-email")]
    [ProducesResponseType(typeof(MarkEmailVerifiedResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkEmailVerified([FromRoute] Guid userId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentUserId))
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            var verifiedAt = await markEmailVerifiedService.MarkEmailVerifiedAsync(userId, currentUserId, ClientRequestInfo.From(HttpContext), cancellationToken);
            return Ok(new MarkEmailVerifiedResponse { EmailVerifiedAt = verifiedAt });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public class MarkEmailVerifiedResponse
{
    public DateTime EmailVerifiedAt { get; set; }
}
