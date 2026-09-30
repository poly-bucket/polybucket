using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Features.Authentication.ResendVerificationEmail.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.ResendVerificationEmail.Http
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class ResendVerificationEmailController(
        ResendVerificationEmailCommandHandler handler,
        ILogger<ResendVerificationEmailController> logger) : ControllerBase
    {
        public const string AcceptedMessage = "If the address belongs to an unverified account, a new verification link has been sent";

        private readonly ResendVerificationEmailCommandHandler _handler = handler;
        private readonly ILogger<ResendVerificationEmailController> _logger = logger;

        /// <summary>
        /// Sends a fresh email verification link and invalidates any previous one.
        /// </summary>
        /// <remarks>
        /// Always returns 202 so the endpoint cannot be used to discover which addresses are registered.
        /// Requests for the same address are throttled to one per minute.
        /// </remarks>
        /// <param name="command">The address to send the verification link to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="202">The request was accepted.</response>
        /// <response code="400">The email address is not valid.</response>
        [HttpPost("verify-email/resend")]
        [EnableRateLimiting(RequestSecurityServiceCollectionExtensions.AuthStrictPolicy)]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [ProducesResponseType(429)]
        public async Task<IActionResult> Resend([FromBody] ResendVerificationEmailCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                command.Client = ClientRequestInfo.From(HttpContext);
                await _handler.Handle(command, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while resending the verification email");
            }

            return Accepted(new { message = AcceptedMessage });
        }
    }
}
