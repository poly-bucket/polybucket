using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleThemeComponents.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleThemeComponents.Http;

[ApiController]
[Route("api/system-settings/extensible-theme")]
public class GetActiveExtensibleThemeComponentsController(IGetActiveExtensibleThemeComponentsService service) : ControllerBase
{
    [HttpGet("components")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ThemeComponents), 200)]
    public async Task<ActionResult<ThemeComponents>> GetActiveThemeComponents(CancellationToken cancellationToken)
    {
        var result = await service.GetActiveExtensibleThemeComponentsAsync(cancellationToken);
        if (result == null) return NotFound("No theme components found");
        return Ok(result);
    }
}
