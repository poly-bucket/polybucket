using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateAuthenticationSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.UpdateAuthenticationSettings.Http;

[ApiController]
[Route("api/system-settings/auth")]
[Authorize(Roles = "Admin")]
public class UpdateAuthenticationSettingsController(IUpdateAuthenticationSettingsService updateAuthenticationSettingsService) : ControllerBase
{
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
    public async Task<ActionResult<object>> UpdateAuthenticationSettings([FromBody] AuthenticationSettings settings, CancellationToken cancellationToken)
    {
        if (!settings.IsValid())
        {
            return BadRequest(new { message = "Invalid authentication settings provided" });
        }

        var success = await updateAuthenticationSettingsService.UpdateAuthenticationSettingsAsync(settings, cancellationToken);
        if (success)
        {
            return Ok(new { message = "Authentication settings updated successfully" });
        }

        return BadRequest(new { message = "Failed to update authentication settings" });
    }
}
