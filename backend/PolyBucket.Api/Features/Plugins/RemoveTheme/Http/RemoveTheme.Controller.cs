using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.RemoveTheme.Domain;

namespace PolyBucket.Api.Features.Plugins.RemoveTheme.Http;

[ApiController]
[Route("api/plugins/themes")]
[Authorize]
public class RemoveThemeController(IRemoveThemeService service) : ControllerBase
{
    [HttpDelete("{pluginId}")]
    public async Task<ActionResult<ThemeApplicationResult>> RemoveTheme(string pluginId)
    {
        var outcome = await service.RemoveAsync(pluginId);
        return outcome.Status == ThemeRequestStatus.BadRequest ? BadRequest(outcome.Value) : Ok(outcome.Value);
    }
}
