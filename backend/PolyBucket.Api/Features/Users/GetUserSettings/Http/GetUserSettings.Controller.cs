using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.GetUserSettings.Domain;

namespace PolyBucket.Api.Features.Users.GetUserSettings.Http;

[ApiController]
[Route("api/users/settings")]
[Authorize]
public class GetUserSettingsController : ControllerBase
{
    private readonly IGetUserSettingsService _getUserSettingsService;
    private readonly ILogger<GetUserSettingsController> _logger;

    public GetUserSettingsController(IGetUserSettingsService getUserSettingsService, ILogger<GetUserSettingsController> logger)
    {
        _getUserSettingsService = getUserSettingsService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user settings
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetUserSettingsResult), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<GetUserSettingsResult>> GetUserSettings(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized();
            }

            var result = await _getUserSettingsService.GetUserSettingsAsync(
                new GetUserSettingsRequest { UserId = userGuid },
                cancellationToken);
            return Ok(result);
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
}
