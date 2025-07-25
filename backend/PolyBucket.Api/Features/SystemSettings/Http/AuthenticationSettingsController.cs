using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Features.SystemSettings.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.Http;

[ApiController]
[Route("api/system-settings/auth")]
public class AuthenticationSettingsController(
    IAuthenticationSettingsService authenticationSettingsService,
    ILogger<AuthenticationSettingsController> logger) : ControllerBase
{
    private readonly IAuthenticationSettingsService _authenticationSettingsService = authenticationSettingsService;
    private readonly ILogger<AuthenticationSettingsController> _logger = logger;

    /// <summary>
    /// Get the current authentication settings
    /// </summary>
    /// <returns>Current authentication configuration</returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(AuthenticationSettings))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<AuthenticationSettings>> GetAuthenticationSettings()
    {
        try
        {
            var settings = await _authenticationSettingsService.GetAuthenticationSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authentication settings");
            return StatusCode(500, new { message = "An unexpected error occurred while getting authentication settings" });
        }
    }

    /// <summary>
    /// Update the authentication settings
    /// </summary>
    /// <param name="settings">Authentication settings configuration</param>
    /// <returns>Update result</returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(object))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<object>> UpdateAuthenticationSettings([FromBody] AuthenticationSettings settings)
    {
        try
        {
            if (!settings.IsValid())
            {
                return BadRequest(new { message = "Invalid authentication settings provided" });
            }

            var success = await _authenticationSettingsService.UpdateAuthenticationSettingsAsync(settings);
            
            if (success)
            {
                return Ok(new { message = "Authentication settings updated successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to update authentication settings" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating authentication settings");
            return StatusCode(500, new { message = "An unexpected error occurred while updating authentication settings" });
        }
    }
} 