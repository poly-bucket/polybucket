using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.ResetPassword.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.ResetPassword.Http
{
    [ApiController]
    [Route("api/auth")]
    public class ResetPasswordController : ControllerBase
    {
        private readonly ResetPasswordCommandHandler _handler;
        private readonly ILogger<ResetPasswordController> _logger;

        public ResetPasswordController(ResetPasswordCommandHandler handler, ILogger<ResetPasswordController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        /// <summary>
        /// Reset password using a valid reset token
        /// </summary>
        /// <param name="command">Password reset details with token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success response</returns>
        [HttpPost("reset-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(400, Type = typeof(object))]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _handler.Handle(command, cancellationToken);
                return Ok(new { message = "Password has been reset successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Password reset failed for {Email}", command.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for {Email}", command.Email);
                return StatusCode(500, new { message = "An unexpected error occurred during password reset" });
            }
        }
    }
} 