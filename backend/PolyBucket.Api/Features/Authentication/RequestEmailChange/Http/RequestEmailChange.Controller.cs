using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Features.Authentication.RequestEmailChange.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.RequestEmailChange.Http
{
    [ApiController]
    [Route("api/auth")]
    [Authorize]
    public class RequestEmailChangeController(RequestEmailChangeCommandHandler handler) : ControllerBase
    {
        private readonly RequestEmailChangeCommandHandler _handler = handler;

        /// <summary>
        /// Starts changing the signed-in user's email address.
        /// </summary>
        /// <remarks>
        /// The address is not changed until the user opens the confirmation link sent to the new address.
        /// Once confirmed, a notice is sent to the previous address.
        /// </remarks>
        /// <param name="command">The new address and the user's current password.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="202">A confirmation link was sent to the new address.</response>
        /// <response code="400">The address is invalid, unchanged, already in use, or email delivery is not configured.</response>
        /// <response code="401">The current password is incorrect or the user is not signed in.</response>
        [HttpPost("email-change")]
        [EnableRateLimiting(RequestSecurityServiceCollectionExtensions.AuthStrictPolicy)]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(429)]
        public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!Guid.TryParse(User.FindUserIdClaim(), out var userId))
            {
                return Unauthorized();
            }

            try
            {
                command.UserId = userId;
                command.Client = ClientRequestInfo.From(HttpContext);
                await _handler.Handle(command, cancellationToken);
                return Accepted(new { message = "Check your new inbox for a confirmation link", pendingEmail = command.NewEmail.Trim() });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
