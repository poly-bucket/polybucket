using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Authentication.GetActiveSessions.Domain;

namespace PolyBucket.Api.Features.Authentication.GetActiveSessions.Http;

[ApiController]
[Route("api/auth/account/sessions")]
[Authorize]
public class GetActiveSessionsController(IGetActiveSessionsService getActiveSessionsService) : ControllerBase
{
    /// <summary>
    /// Get active refresh-token-backed sessions for the signed-in user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetActiveSessionsResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<GetActiveSessionsResponse>> GetActiveSessions(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        var response = await getActiveSessionsService.GetActiveSessionsAsync(userId, cancellationToken);
        return Ok(response);
    }
}
