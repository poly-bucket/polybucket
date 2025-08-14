using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.Queries.GetUserProfile;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.Queries.GetUserProfile
{
    [ApiController]
    [Route("api/users/profile")]
    public class GetUserProfileController : ControllerBase
    {
        private readonly GetUserProfileQueryHandler _handler;
        private readonly ILogger<GetUserProfileController> _logger;

        public GetUserProfileController(GetUserProfileQueryHandler handler, ILogger<GetUserProfileController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        /// <summary>
        /// Get user profile by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User profile information or private profile response</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(GetUserProfileResponse))]
        [ProducesResponseType(200, Type = typeof(PrivateProfileResponse))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<object>> GetUserProfileById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetUserProfileQuery { Id = id };
                var response = await _handler.Handle(query, cancellationToken);
                
                if (response is PrivateProfileResponse privateResponse)
                {
                    return Ok(privateResponse);
                }
                
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User profile not found for ID {UserId}", id);
                return NotFound(new { message = "User profile not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile for ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the user profile" });
            }
        }

        /// <summary>
        /// Get user profile by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User profile information or private profile response</returns>
        [HttpGet("by-username/{username}")]
        [ProducesResponseType(200, Type = typeof(GetUserProfileResponse))]
        [ProducesResponseType(200, Type = typeof(PrivateProfileResponse))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<object>> GetUserProfileByUsername(
            [FromRoute] string username,
            CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetUserProfileQuery { Username = username };
                var response = await _handler.Handle(query, cancellationToken);
                
                if (response is PrivateProfileResponse privateResponse)
                {
                    return Ok(privateResponse);
                }
                
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User profile not found for username {Username}", username);
                return NotFound(new { message = "User profile not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile for username {Username}", username);
                return StatusCode(500, new { message = "An error occurred while retrieving the user profile" });
            }
        }
    }
}
