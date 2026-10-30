using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.ApplyTheme.Http;

[ApiController]
[Route("api/plugins/themes")]
[Authorize]
public class ApplyThemeController(IApplyThemeService service) : ControllerBase
{
    [HttpPost("{pluginId}/apply")]
    public ActionResult<ThemeApplicationResult> ApplyTheme(string pluginId)
    {
        var outcome = service.Apply(pluginId);
        return outcome.Status == ThemeRequestStatus.BadRequest ? BadRequest(outcome.ErrorBody) : Ok(outcome.Value);
    }
}
