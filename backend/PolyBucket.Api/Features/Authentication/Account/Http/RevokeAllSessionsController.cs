using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Authentication.Repository;

namespace PolyBucket.Api.Features.Authentication.Account.Http;

[ApiController]
[Route("api/auth/account/sessions")]
[Authorize]
public class RevokeAllSessionsController(IAuthenticationRepository authRepository) : ControllerBase
{
    private readonly IAuthenticationRepository _authRepository = authRepository;

    /// <summary>
    /// Revoke all refresh tokens for the signed-in user (sign out everywhere). The current browser session will lose refresh capability after tokens expire or on next refresh attempt.
    /// </summary>
    [HttpPost("revoke-all")]
    [ProducesResponseType(typeof(RevokeAllSessionsResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<RevokeAllSessionsResponse>> RevokeAll(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        await _authRepository.RevokeAllRefreshTokensForUserAsync(
            userId,
            "User revoked all sessions",
            "127.0.0.1");

        return Ok(new RevokeAllSessionsResponse { Success = true });
    }
}

public class RevokeAllSessionsResponse
{
    public bool Success { get; set; }
}
