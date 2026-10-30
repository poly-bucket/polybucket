using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Features.SystemSettings.UpdateTokenSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.UpdateTokenSettings.Http;

[ApiController]
[Route("api/system-settings/token")]
[Authorize(Roles = "Admin")]
public class UpdateTokenSettingsController(IUpdateTokenSettingsService updateTokenSettingsService) : ControllerBase
{
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
    public async Task<ActionResult<object>> UpdateTokenSettings([FromBody] TokenSettings settings, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var success = await updateTokenSettingsService.UpdateTokenSettingsAsync(settings, cancellationToken);
            if (success)
            {
                return Ok(new { message = "Token settings updated successfully" });
            }

            return BadRequest(new { message = "Failed to update token settings" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
