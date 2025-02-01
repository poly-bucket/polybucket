using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Controllers.Users.UserSettings.Domain;

namespace Api.Controllers.Users.UserSettings.Http;

[ApiController]
[Route("api/users/settings")]
[Authorize]
public class UserSettingsController : ControllerBase
{
    private readonly IGetUserSettingsService _getUserSettingsService;
    private readonly IUpdateUserSettingsService _updateUserSettingsService;
    private readonly ILogger<UserSettingsController> _logger;

    public UserSettingsController(
        IGetUserSettingsService getUserSettingsService,
        IUpdateUserSettingsService updateUserSettingsService,
        ILogger<UserSettingsController> logger)
    {
        _getUserSettingsService = getUserSettingsService;
        _updateUserSettingsService = updateUserSettingsService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(200, Type = typeof(GetUserSettingsResponse))]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetUserSettingsResponse>> GetUserSettings()
    {
        try
        {
            // Debug logging for claims
            foreach (var claim in User.Claims)
            {
                _logger.LogDebug("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            var userId = User.FindFirst("sub")?.Value;
            _logger.LogDebug("Found userId from sub claim: {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No 'sub' claim found in token");
                return Unauthorized();
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Failed to parse userId '{UserId}' as GUID", userId);
                return Unauthorized();
            }

            _logger.LogDebug("Parsed userGuid: {UserGuid}", userGuid);

            var request = new GetUserSettingsRequest { UserId = userGuid };
            var settings = await _getUserSettingsService.ExecuteAsync(request);
            return Ok(settings);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user settings");
            return StatusCode(500, new { message = "An error occurred while retrieving user settings" });
        }
    }

    [HttpPut]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> UpdateUserSettings([FromBody] UpdateUserSettingsRequest request)
    {
        try
        {
            // Debug logging for claims
            foreach (var claim in User.Claims)
            {
                _logger.LogDebug("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            var userId = User.FindFirst("sub")?.Value;
            _logger.LogDebug("Found userId from sub claim: {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No 'sub' claim found in token");
                return Unauthorized();
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Failed to parse userId '{UserId}' as GUID", userId);
                return Unauthorized();
            }

            _logger.LogDebug("Parsed userGuid: {UserGuid}", userGuid);

            request.UserId = userGuid;
            await _updateUserSettingsService.ExecuteAsync(request);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user settings");
            return StatusCode(500, new { message = "An error occurred while updating user settings" });
        }
    }
} 