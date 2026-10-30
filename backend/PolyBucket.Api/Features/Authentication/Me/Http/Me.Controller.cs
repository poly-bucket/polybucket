using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Authentication.Me.Domain;

namespace PolyBucket.Api.Features.Authentication.Me.Http;

[ApiController]
[Route("api/auth")]
[Authorize]
public class MeController(IMeService meService) : ControllerBase
{
    /// <summary>
    /// Returns the profile of the signed-in user, including email verification state.
    /// </summary>
    /// <response code="200">The current user.</response>
    /// <response code="401">The access token is missing or invalid.</response>
    /// <response code="404">The user no longer exists.</response>
    [HttpGet("me")]
    [ProducesResponseType(200, Type = typeof(MeResponse))]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<MeResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid authentication token");
        }

        var response = await meService.GetCurrentUserAsync(userId, cancellationToken);
        if (response == null)
        {
            return NotFound("User not found");
        }

        return Ok(response);
    }
}
