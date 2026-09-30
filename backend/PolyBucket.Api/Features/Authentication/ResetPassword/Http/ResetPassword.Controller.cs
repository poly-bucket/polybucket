using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Features.Authentication.ResetPassword.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.ResetPassword.Http
{
    [ApiController]
    [Route("api/auth")]
    public class ResetPasswordController(ResetPasswordCommandHandler handler, ILogger<ResetPasswordController> logger) : ControllerBase
    {
        private readonly ResetPasswordCommandHandler _handler = handler;
        private readonly ILogger<ResetPasswordController> _logger = logger;

        /// <summary>
        /// Sets a new password using the one-time token from a password reset or account invite link.
        /// </summary>
        /// <remarks>
        /// The token is single-use. A successful reset signs the user out everywhere, invalidates every other
        /// outstanding reset link, marks the address as verified, and sends a "password changed" notice.
        /// </remarks>
        /// <param name="command">The token from the link and the new password.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">The password was changed.</response>
        /// <response code="400">The token is invalid, expired, or already used, or the password is invalid.</response>
        [HttpPost("reset-password")]
        [EnableRateLimiting(RequestSecurityServiceCollectionExtensions.AuthStrictPolicy)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(object))]
        [ProducesResponseType(429)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                command.Client = ClientRequestInfo.From(HttpContext);
                await _handler.Handle(command, cancellationToken);
                return Ok(new { message = "Password has been reset successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Password reset failed: {Reason}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, new { message = "An unexpected error occurred during password reset" });
            }
        }
    }
}
