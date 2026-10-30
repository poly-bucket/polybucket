using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Http;

[ApiController]
[Route("api/system-settings/theme")]
public class GetThemeSettingsController(IGetThemeSettingsService getThemeSettingsService) : ControllerBase
{
    /// <summary>
    /// Get the current theme settings
    /// </summary>
    /// <returns>Current theme configuration</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(200, Type = typeof(GetThemeSettingsResponse))]
    [ProducesResponseType(400)]
    public async Task<ActionResult<GetThemeSettingsResponse>> GetThemeSettings(CancellationToken cancellationToken)
    {
        var result = await getThemeSettingsService.GetThemeSettingsAsync(cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
