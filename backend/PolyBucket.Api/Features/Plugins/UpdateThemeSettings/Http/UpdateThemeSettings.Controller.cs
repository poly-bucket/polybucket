using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.UpdateThemeSettings.Domain;

namespace PolyBucket.Api.Features.Plugins.UpdateThemeSettings.Http;

[ApiController]
[Route("api/plugins/themes")]
[Authorize]
public class UpdateThemeSettingsController(IUpdateThemeSettingsService service) : ControllerBase
{
    [HttpPut("{pluginId}/settings")]
    public async Task<ActionResult<ThemeApplicationResult>> UpdateThemeSettings(string pluginId, [FromBody] ThemeSettings settings)
    {
        var outcome = await service.UpdateAsync(pluginId, settings);
        return outcome.Status == ThemeRequestStatus.BadRequest ? BadRequest(outcome.Value) : Ok(outcome.Value);
    }
}
