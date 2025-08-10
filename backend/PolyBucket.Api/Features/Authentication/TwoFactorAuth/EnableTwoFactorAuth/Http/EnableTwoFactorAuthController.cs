using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Http
{
    [ApiController]
    [Route("api/auth/2fa")]
    [Authorize]
    public class EnableTwoFactorAuthController : ControllerBase
    {
        private readonly EnableTwoFactorAuthCommandHandler _handler;
        private readonly ILogger<EnableTwoFactorAuthController> _logger;

        public EnableTwoFactorAuthController(
            EnableTwoFactorAuthCommandHandler handler,
            ILogger<EnableTwoFactorAuthController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpPost("enable")]
        [ProducesResponseType(200, Type = typeof(EnableTwoFactorAuthResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Enable([FromBody] EnableTwoFactorAuthCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("EnableTwoFactorAuthController.Enable: Enabling 2FA for user {UserId}", command.UserId);
            try
            {
                // Get user ID from the authenticated user using the standard claim type
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("EnableTwoFactorAuthController.Enable: No user ID found in token. Available claims: {Claims}", 
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                command.UserId = userId;

                var response = await _handler.Handle(command, cancellationToken);
                
                if (response.Success)
                {
                    _logger.LogInformation("EnableTwoFactorAuthController.Enable: 2FA enabled successfully for user {UserId}", command.UserId);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("EnableTwoFactorAuthController.Enable: 2FA enablement failed for user {UserId}", command.UserId);
                    return BadRequest(response);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "EnableTwoFactorAuthController.Enable: Invalid operation for 2FA enablement");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EnableTwoFactorAuthController.Enable: Error during 2FA enablement");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
} 