using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Authentication.Repository;

namespace PolyBucket.Api.Features.Authentication.Account.Http;

[ApiController]
[Route("api/auth/account/sessions")]
[Authorize]
public class RevokeSessionController(IAuthenticationRepository authRepository) : ControllerBase
{
    private readonly IAuthenticationRepository _authRepository = authRepository;

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

        var session = await _authRepository.GetActiveRefreshTokenByIdAsync(sessionId, userId);
        if (session == null)
        {
            return NotFound(new { message = "Session not found" });
        }

        await _authRepository.RevokeRefreshTokenByIdAsync(sessionId, "User revoked one session", "127.0.0.1");
        return Ok(new RevokeSessionResponse { Success = true });
    }
}

public class RevokeSessionResponse
{
    public bool Success { get; set; }
}
