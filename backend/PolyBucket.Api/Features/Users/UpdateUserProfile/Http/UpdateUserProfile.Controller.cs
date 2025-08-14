using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.UpdateUserProfile.Domain;
using PolyBucket.Api.Features.Users.UpdateUserProfile.Handlers;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.UpdateUserProfile.Http
{
    [ApiController]
    [Route("api/users/profile/update")]
    [Authorize]
    public class UpdateUserProfileController : ControllerBase
    {
        private readonly UpdateUserProfileCommandHandler _handler;
        private readonly ILogger<UpdateUserProfileController> _logger;

        public UpdateUserProfileController(UpdateUserProfileCommandHandler handler, ILogger<UpdateUserProfileController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> UpdateUserProfile([FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized();
                }

                var command = new UpdateUserProfileCommand
                {
                    UserId = userGuid,
                    Bio = request.Bio,
                    Country = request.Country,
                    WebsiteUrl = request.WebsiteUrl,
                    TwitterUrl = request.TwitterUrl,
                    InstagramUrl = request.InstagramUrl,
                    YouTubeUrl = request.YouTubeUrl,
                    IsProfilePublic = request.IsProfilePublic,
                    ShowEmail = request.ShowEmail,
                    ShowLastLogin = request.ShowLastLogin,
                    ShowStatistics = request.ShowStatistics
                };

                await _handler.Handle(command, cancellationToken);
                return Ok(new { message = "User profile updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to update user profile");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new { message = "An error occurred while updating the user profile" });
            }
        }
    }
}
