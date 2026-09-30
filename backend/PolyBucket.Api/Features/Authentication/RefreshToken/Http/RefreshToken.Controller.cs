using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Features.Authentication.RefreshToken.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.RefreshToken.Http
{
    [ApiController]
    [Route("api/auth")]
    public class RefreshTokenController(RefreshTokenCommandHandler handler, ILogger<RefreshTokenController> logger) : ControllerBase
    {
        private readonly RefreshTokenCommandHandler _handler = handler;
        private readonly ILogger<RefreshTokenController> _logger = logger;

        /// <summary>
        /// Exchange a valid refresh token for a new access and refresh token pair
        /// </summary>
        /// <param name="command">The refresh token to rotate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New authentication tokens</returns>
        [HttpPost("refresh-token")]
        [EnableRateLimiting(RequestSecurityServiceCollectionExtensions.AuthStandardPolicy)]
        [ProducesResponseType(200, Type = typeof(RefreshTokenCommandResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(429)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                command.Client = ClientRequestInfo.From(HttpContext);
                var response = await _handler.Handle(command, cancellationToken);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Token refresh failed");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { message = "An unexpected error occurred during token refresh" });
            }
        }
    }
} 