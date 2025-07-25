using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Http
{
    [ApiController]
    [Route("api/auth/2fa")]
    [Authorize]
    public class GetTwoFactorAuthStatusController : ControllerBase
    {
        private readonly GetTwoFactorAuthStatusQueryHandler _handler;
        private readonly ILogger<GetTwoFactorAuthStatusController> _logger;

        public GetTwoFactorAuthStatusController(
            GetTwoFactorAuthStatusQueryHandler handler,
            ILogger<GetTwoFactorAuthStatusController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpGet("status")]
        [ProducesResponseType(200, Type = typeof(GetTwoFactorAuthStatusResponse))]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
        {
            try
            {
                // Get user ID from the authenticated user using the standard claim type
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("GetTwoFactorAuthStatusController.GetStatus: No user ID found in token. Available claims: {Claims}", 
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                var query = new GetTwoFactorAuthStatusQuery { UserId = userId };
                var response = await _handler.Handle(query, cancellationToken);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting 2FA status");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
} 