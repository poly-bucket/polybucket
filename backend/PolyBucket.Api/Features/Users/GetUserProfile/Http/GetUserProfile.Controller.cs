using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.GetUserProfile.Domain;

namespace PolyBucket.Api.Features.Users.GetUserProfile.Http;

[ApiController]
[Route("api/users/profile")]
public class GetUserProfileController : ControllerBase
{
    private readonly IGetUserProfileService _getUserProfileService;
    private readonly ILogger<GetUserProfileController> _logger;

    public GetUserProfileController(IGetUserProfileService getUserProfileService, ILogger<GetUserProfileController> logger)
    {
        _getUserProfileService = getUserProfileService;
        _logger = logger;
    }

    /// <summary>
    /// Get user profile by ID.
    /// Private profiles are visible to the profile owner and admins; other viewers receive a private-profile response.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(200, Type = typeof(GetUserProfileResponse))]
    [ProducesResponseType(200, Type = typeof(PrivateProfileResponse))]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<object>> GetUserProfileById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetUserProfileQuery { Id = id };
            PopulateRequesterContext(query);
            var response = await _getUserProfileService.GetUserProfileAsync(query, cancellationToken);

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
    /// Get user profile by username.
    /// Private profiles are visible to the profile owner and admins; other viewers receive a private-profile response.
    /// </summary>
    [HttpGet("by-username/{username}")]
    [ProducesResponseType(200, Type = typeof(GetUserProfileResponse))]
    [ProducesResponseType(200, Type = typeof(PrivateProfileResponse))]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<object>> GetUserProfileByUsername(
        [FromRoute] string username,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetUserProfileQuery { Username = username };
            PopulateRequesterContext(query);
            var response = await _getUserProfileService.GetUserProfileAsync(query, cancellationToken);

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

    private void PopulateRequesterContext(GetUserProfileQuery query)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            query.RequestingUserId = userId;
        }

        query.IsRequestingUserAdmin = User.IsInRole("Admin");
    }
}
