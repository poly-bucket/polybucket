using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetThemeSettings.Domain;

namespace PolyBucket.Api.Features.Plugins.GetThemeSettings.Http;

[ApiController]
[Route("api/plugins/themes")]
[Authorize]
public class GetThemeSettingsController(IGetThemeSettingsService service) : ControllerBase
{
    [HttpGet("{pluginId}/settings")]
    public ActionResult<ThemeSettings> GetThemeSettings(string pluginId)
    {
        var outcome = service.GetSettings(pluginId);
        return outcome.Status == ThemeRequestStatus.BadRequest ? BadRequest(outcome.ErrorBody) : Ok(outcome.Value);
    }
}
