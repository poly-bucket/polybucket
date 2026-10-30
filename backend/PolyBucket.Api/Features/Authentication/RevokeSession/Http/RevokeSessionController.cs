using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Authentication.RevokeSession.Domain;

namespace PolyBucket.Api.Features.Authentication.RevokeSession.Http;

[ApiController]
[Route("api/auth/account/sessions")]
[Authorize]
public class RevokeSessionController(IRevokeSessionService revokeSessionService) : ControllerBase
{
    /// <summary>
    /// Revoke one active refresh-token-backed session for the signed-in user.
    /// </summary>
    [HttpPost("{sessionId:guid}/revoke")]
    [ProducesResponseType(typeof(RevokeSessionResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<RevokeSessionResponse>> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        var result = await revokeSessionService.RevokeSessionAsync(userId, sessionId, cancellationToken);
        if (!result.Found)
        {
            return NotFound(new { message = "Session not found" });
        }

        return Ok(new RevokeSessionResponse { Success = true });
    }
}

public class RevokeSessionResponse
{
    public bool Success { get; set; }
}
