using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.VerifyEmail.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.VerifyEmail.Http
{
    [ApiController]
    [Route("api/auth")]
    public class VerifyEmailController : ControllerBase
    {
        private readonly VerifyEmailCommandHandler _handler;
        private readonly ILogger<VerifyEmailController> _logger;

        public VerifyEmailController(VerifyEmailCommandHandler handler, ILogger<VerifyEmailController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpPost("verify-email")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _handler.Handle(command, cancellationToken);
                return Ok(new { message = "Email verified successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Email verification failed for {Email}", command.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for {Email}", command.Email);
                return StatusCode(500, new { message = "An unexpected error occurred during email verification" });
            }
        }
    }
} 