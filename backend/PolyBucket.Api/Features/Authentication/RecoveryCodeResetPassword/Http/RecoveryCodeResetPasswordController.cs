using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Features.Authentication.RecoveryCodeResetPassword.Domain;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodeResetPassword.Http;

[ApiController]
[Route("api/auth")]
public class RecoveryCodeResetPasswordController(
    RecoveryCodeResetPasswordCommandHandler handler,
    ILogger<RecoveryCodeResetPasswordController> logger) : ControllerBase
{
    /// <summary>
    /// Sets a new password using a single-use account recovery code when email reset is unavailable.
    /// </summary>
    /// <param name="command">Account identifier, recovery code, and new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The password was changed.</response>
    /// <response code="400">The recovery code or account identifier is invalid.</response>
    [HttpPost("recovery-code/reset-password")]
    [EnableRateLimiting(RequestSecurityServiceCollectionExtensions.AuthStrictPolicy)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> ResetPasswordWithRecoveryCode(
        [FromBody] RecoveryCodeResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            command.Client = ClientRequestInfoMapper.From(HttpContext);
            await handler.Handle(command, cancellationToken);
            return Ok(new { message = "Password has been reset successfully" });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Recovery code password reset failed: {Reason}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during recovery code password reset");
            return StatusCode(500, new { message = "An unexpected error occurred during password reset" });
        }
    }
}
