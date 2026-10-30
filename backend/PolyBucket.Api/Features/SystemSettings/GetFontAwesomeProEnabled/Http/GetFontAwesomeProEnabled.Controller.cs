using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.GetFontAwesomeProEnabled.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetFontAwesomeProEnabled.Http;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class GetFontAwesomeProEnabledController(IGetFontAwesomeProEnabledService getFontAwesomeProEnabledService) : ControllerBase
{
    [HttpGet("fontawesome-settings/pro-enabled")]
    [ProducesResponseType(200, Type = typeof(bool))]
    public async Task<ActionResult<bool>> GetFontAwesomeProEnabled(CancellationToken cancellationToken)
    {
        return Ok(await getFontAwesomeProEnabledService.IsFontAwesomeProEnabledAsync(cancellationToken));
    }
}
