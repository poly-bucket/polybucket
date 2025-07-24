using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.Services;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    [ApiController]
    [Route("api/federation/invite")]
    [Authorize]
    public class GenerateInviteUrlController(IFederationService federationService) : ControllerBase
    {
        private readonly IFederationService _federationService = federationService;

        /// <summary>
        /// Generate a new federation invite URL
        /// </summary>
        [HttpPost]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(InviteUrlResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<InviteUrlResponse>> GenerateInviteUrl([FromBody] GenerateInviteUrlRequest? request = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid authentication token");
            }

            try
            {
                var expiresAt = request?.ExpiresAt ?? DateTime.UtcNow.AddDays(7);
                
                // Validate expiration date
                if (expiresAt <= DateTime.UtcNow)
                {
                    return BadRequest("Expiration date must be in the future");
                }

                if (expiresAt > DateTime.UtcNow.AddDays(365))
                {
                    return BadRequest("Expiration date cannot be more than 365 days in the future");
                }

                var inviteUrl = await _federationService.GenerateInviteUrlAsync(userId, expiresAt);
                
                return Ok(new InviteUrlResponse
                {
                    InviteUrl = inviteUrl,
                    ExpiresAt = expiresAt,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to generate invite URL: {ex.Message}");
            }
        }
    }

    public class GenerateInviteUrlRequest
    {
        /// <summary>
        /// Optional expiration date for the invite URL. Defaults to 7 days from now.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    public class InviteUrlResponse
    {
        public string InviteUrl { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
} 