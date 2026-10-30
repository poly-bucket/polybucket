using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleTheme.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleTheme.Http;

[ApiController]
[Route("api/system-settings/extensible-theme")]
public class GetActiveExtensibleThemeController(IGetActiveExtensibleThemeService service) : ControllerBase
{
    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ThemeDefinition), 200)]
    public async Task<ActionResult<ThemeDefinition>> GetActiveTheme(CancellationToken cancellationToken)
    {
        var result = await service.GetActiveExtensibleThemeAsync(cancellationToken);
        if (result == null) return NotFound("No active theme found");
        return Ok(result);
    }
}
