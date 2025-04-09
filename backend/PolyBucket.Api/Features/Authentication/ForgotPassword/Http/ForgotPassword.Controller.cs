using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.ForgotPassword.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.ForgotPassword.Http
{
    [ApiController]
    [Route("api/auth")]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly ForgotPasswordCommandHandler _handler;
        private readonly ILogger<ForgotPasswordController> _logger;

        public ForgotPasswordController(ForgotPasswordCommandHandler handler, ILogger<ForgotPasswordController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        /// <summary>
        /// Request a password reset email
        /// </summary>
        /// <param name="command">Email address for password reset</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success response (always returns 200 for security)</returns>
        [HttpPost("forgot-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _handler.Handle(command, cancellationToken);
                // Always return success to prevent email enumeration
                return Ok(new { message = "If the email address exists, a password reset link has been sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password request for {Email}", command.Email);
                // Still return success to prevent email enumeration
                return Ok(new { message = "If the email address exists, a password reset link has been sent" });
            }
        }
    }
} 