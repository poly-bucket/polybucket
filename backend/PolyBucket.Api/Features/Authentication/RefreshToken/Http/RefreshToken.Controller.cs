using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        [HttpPost("refresh-token")]
        [ProducesResponseType(200, Type = typeof(RefreshTokenCommandResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
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