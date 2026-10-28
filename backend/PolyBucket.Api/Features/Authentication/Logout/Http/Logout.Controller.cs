using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Features.Authentication.Logout.Domain;

namespace PolyBucket.Api.Features.Authentication.Logout.Http;

[ApiController]
[Route("api/auth")]
public class LogoutController(LogoutCommandHandler handler, ILogger<LogoutController> logger) : ControllerBase
{
    private readonly LogoutCommandHandler _handler = handler;
    private readonly ILogger<LogoutController> _logger = logger;

    /// <summary>
    /// Revoke the refresh token supplied in the request body (used by the web BFF on sign-out).
    /// </summary>
    /// <param name="command">Refresh token to revoke</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>204 when the request is accepted</returns>
    [HttpPost("logout")]
    [EnableRateLimiting(RequestSecurityServiceCollectionExtensions.AuthStandardPolicy)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        try
        {
            command.Client = ClientRequestInfo.From(HttpContext);
            await _handler.Handle(command, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return NoContent();
        }
    }
}
