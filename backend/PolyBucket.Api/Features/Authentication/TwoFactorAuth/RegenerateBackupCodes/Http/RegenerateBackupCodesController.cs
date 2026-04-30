using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Http
{
    [ApiController]
    [Route("api/auth/2fa")]
    [Authorize]
    public class RegenerateBackupCodesController : ControllerBase
    {
        private readonly RegenerateBackupCodesCommandHandler _handler;
        private readonly ILogger<RegenerateBackupCodesController> _logger;

        public RegenerateBackupCodesController(
            RegenerateBackupCodesCommandHandler handler,
            ILogger<RegenerateBackupCodesController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpPost("regenerate-backup-codes")]
        [ProducesResponseType(200, Type = typeof(RegenerateBackupCodesResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegenerateBackupCodes([FromBody] RegenerateBackupCodesCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Get user ID from the authenticated user using the standard claim type
                var userIdClaim = User.FindUserIdClaim();
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authenticatedUserId))
                {
                    _logger.LogWarning("RegenerateBackupCodesController.RegenerateBackupCodes: No user ID found in token. Available claims: {Claims}", 
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
                    _logger.LogInformation("RegenerateBackupCodesController.RegenerateBackupCodes: Backup codes regenerated successfully for user {UserId}", command.UserId);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("RegenerateBackupCodesController.RegenerateBackupCodes: Backup code regeneration failed for user {UserId}", command.UserId);
                    return BadRequest(response);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "RegenerateBackupCodesController.RegenerateBackupCodes: Invalid operation for backup code regeneration");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegenerateBackupCodesController.RegenerateBackupCodes: Error during backup code regeneration");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
} 