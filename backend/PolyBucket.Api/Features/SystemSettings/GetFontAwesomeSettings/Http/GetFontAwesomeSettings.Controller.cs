using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Http;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class GetFontAwesomeSettingsController(IGetFontAwesomeSettingsService getFontAwesomeSettingsService) : ControllerBase
{
    /// <summary>
    /// Returns Font Awesome integration settings without exposing the stored license key.
    /// </summary>
    [HttpGet("fontawesome-settings")]
    [ProducesResponseType(200, Type = typeof(GetFontAwesomeSettingsResponseDto))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<GetFontAwesomeSettingsResponseDto>> GetFontAwesomeSettings(CancellationToken cancellationToken)
    {
        var settings = await getFontAwesomeSettingsService.GetFontAwesomeSettingsAsync(cancellationToken);
        return Ok(GetFontAwesomeSettingsResponseDto.FromEntity(settings));
    }
}
