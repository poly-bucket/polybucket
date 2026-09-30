using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.VerifyEmail.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.VerifyEmail.Http
{
    [ApiController]
    [Route("api/auth")]
    public class VerifyEmailController(VerifyEmailCommandHandler handler, ILogger<VerifyEmailController> logger) : ControllerBase
    {
        private readonly VerifyEmailCommandHandler _handler = handler;
        private readonly ILogger<VerifyEmailController> _logger = logger;

        /// <summary>
        /// Confirms an email address using the one-time token from a verification or email-change link.
        /// </summary>
        /// <remarks>
        /// The token is single-use. Links are opened by a page that POSTs here only after the user clicks,
        /// so mail scanners that prefetch links do not consume the token.
        /// </remarks>
        /// <param name="command">The token from the link and, optionally, the address it was sent to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">The address was verified, or the email change was completed.</response>
        /// <response code="400">The token is invalid, expired, or already used.</response>
        [HttpPost("verify-email")]
        [EnableRateLimiting(RequestSecurityServiceCollectionExtensions.AuthStandardPolicy)]
        [ProducesResponseType(typeof(VerifyEmailResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(429)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _handler.Handle(command, cancellationToken);
                return Ok(new VerifyEmailResponse
                {
                    Message = result.Purpose == EmailVerificationPurpose.ChangeAddress
                        ? "Your email address has been changed"
                        : "Email verified successfully",
                    Purpose = result.Purpose,
                    Email = result.Email
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Email verification failed: {Reason}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class VerifyEmailResponse
    {
        public string Message { get; set; } = string.Empty;
        public EmailVerificationPurpose Purpose { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
