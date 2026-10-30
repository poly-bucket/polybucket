using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Http;
using PolyBucket.Api.Features.SystemSettings.UpdateFontAwesomeSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.UpdateFontAwesomeSettings.Http;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class UpdateFontAwesomeSettingsController(IUpdateFontAwesomeSettingsService updateFontAwesomeSettingsService) : ControllerBase
{
    [HttpPost("fontawesome-settings")]
    [ProducesResponseType(200, Type = typeof(GetFontAwesomeSettingsResponseDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<GetFontAwesomeSettingsResponseDto>> UpdateFontAwesomeSettings([FromBody] UpdateFontAwesomeSettingsRequestDto settings, CancellationToken cancellationToken)
    {
        if (settings == null)
        {
            return BadRequest(new { message = "Settings data is required" });
        }
        var updated = await updateFontAwesomeSettingsService.UpdateFontAwesomeSettingsAsync(settings, cancellationToken);
        return Ok(GetFontAwesomeSettingsResponseDto.FromEntity(updated));
    }
}
