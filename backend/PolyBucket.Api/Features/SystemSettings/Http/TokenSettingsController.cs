using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.SystemSettings.Services;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.Http;

[ApiController]
[Route("api/system-settings/token")]
[Authorize(Roles = "Admin")]
public class TokenSettingsController(
    ITokenSettingsService tokenSettingsService,
    ILogger<TokenSettingsController> logger) : ControllerBase
{
    private readonly ITokenSettingsService _tokenSettingsService = tokenSettingsService;
    private readonly ILogger<TokenSettingsController> _logger = logger;

    /// <summary>
    /// Get the current token settings
    /// </summary>
    /// <returns>Current token configuration</returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(TokenSettings))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<TokenSettings>> GetTokenSettings()
    {
        try
        {
            var settings = await _tokenSettingsService.GetTokenSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token settings");
            return StatusCode(500, new { message = "An unexpected error occurred while getting token settings" });
        }
    }

    /// <summary>
    /// Update the token settings
    /// </summary>
    /// <param name="settings">Token settings configuration</param>
    /// <returns>Update result</returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(object))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<object>> UpdateTokenSettings([FromBody] TokenSettings settings)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var success = await _tokenSettingsService.UpdateTokenSettingsAsync(settings);
            
            if (success)
            {
                return Ok(new { message = "Token settings updated successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to update token settings" });
            }
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating token settings");
            return StatusCode(500, new { message = "An unexpected error occurred while updating token settings" });
        }
    }
} 