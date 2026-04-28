using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Services;
using PolyBucket.Api.Features.Users.RegenerateAvatar.Domain;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.RegenerateAvatar.Http
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class RegenerateAvatarController(IRegenerateAvatarService regenerateAvatarService) : ControllerBase
    {
        private readonly IRegenerateAvatarService _regenerateAvatarService = regenerateAvatarService;

        [HttpPost("avatar/regenerate")]
        [ProducesResponseType(200, Type = typeof(RegenerateAvatarResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RegenerateAvatarResponse>> RegenerateAvatar([FromBody] RegenerateAvatarRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Invalid authentication token");
                }

                var response = await _regenerateAvatarService.RegenerateAvatarAsync(userId, request.Salt, request.Avatar);
                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while regenerating the avatar" });
            }
        }
    }
} 