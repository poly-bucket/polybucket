using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Http
{
    [ApiController]
    [Route("api/auth/2fa")]
    [Authorize]
    public class DisableTwoFactorAuthController : ControllerBase
    {
        private readonly DisableTwoFactorAuthCommandHandler _handler;
        private readonly ILogger<DisableTwoFactorAuthController> _logger;

        public DisableTwoFactorAuthController(
            DisableTwoFactorAuthCommandHandler handler,
            ILogger<DisableTwoFactorAuthController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpPost("disable")]
        [ProducesResponseType(200, Type = typeof(DisableTwoFactorAuthResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Disable([FromBody] DisableTwoFactorAuthCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Get user ID from the authenticated user using the standard claim type
                var userIdClaim = User.FindUserIdClaim();
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authenticatedUserId))
                {
                    _logger.LogWarning("DisableTwoFactorAuthController.Disable: No user ID found in token. Available claims: {Claims}", 
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                if (command.UserId != Guid.Empty && command.UserId != authenticatedUserId)
                {
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                command.UserId = authenticatedUserId;

                var response = await _handler.Handle(command, cancellationToken);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for 2FA disablement");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during 2FA disablement");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
} 