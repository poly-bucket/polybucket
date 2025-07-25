using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Http
{
    [ApiController]
    [Route("api/auth/2fa")]
    [Authorize]
    public class InitializeTwoFactorAuthController : ControllerBase
    {
        private readonly InitializeTwoFactorAuthCommandHandler _handler;
        private readonly ILogger<InitializeTwoFactorAuthController> _logger;

        public InitializeTwoFactorAuthController(
            InitializeTwoFactorAuthCommandHandler handler,
            ILogger<InitializeTwoFactorAuthController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpPost("initialize")]
        [ProducesResponseType(200, Type = typeof(InitializeTwoFactorAuthResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Initialize([FromBody] InitializeTwoFactorAuthCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Debug: Log all available claims
                _logger.LogInformation("Available claims in token:");
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
                
                // Get user ID from the authenticated user using the standard claim type
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                _logger.LogInformation("InitializeTwoFactorAuthController.Initialize: User ID: {UserId}", userIdClaim);
                                 
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("InitializeTwoFactorAuthController.Initialize: No user ID found in token. Available claims: {Claims}", 
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                command.UserId = userId;

                var response = await _handler.Handle(command, cancellationToken);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for 2FA initialization");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for 2FA initialization");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during 2FA initialization");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpGet("debug-claims")]
        [ProducesResponseType(200)]
        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
            return Ok(new { claims });
        }
    }
} 