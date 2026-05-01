using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Authentication.Repository;
using System.Linq;

namespace PolyBucket.Api.Features.Authentication.Account.Http;

[ApiController]
[Route("api/auth/account/sessions")]
[Authorize]
public class GetActiveSessionsController(IAuthenticationRepository authRepository) : ControllerBase
{
    private readonly IAuthenticationRepository _authRepository = authRepository;

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

        var sessions = await _authRepository.GetActiveRefreshTokensForUserAsync(userId);
        var response = new GetActiveSessionsResponse
        {
            Sessions = sessions.Select(s => new SessionSummaryDto
            {
                SessionId = s.Id,
                CreatedAt = s.CreatedAt,
                ExpiresAt = s.ExpiresAt,
                CreatedByIp = s.CreatedByIp,
            }).ToList(),
        };

        return Ok(response);
    }
}

public class GetActiveSessionsResponse
{
    public IReadOnlyList<SessionSummaryDto> Sessions { get; set; } = [];
}

public class SessionSummaryDto
{
    public Guid SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
}
