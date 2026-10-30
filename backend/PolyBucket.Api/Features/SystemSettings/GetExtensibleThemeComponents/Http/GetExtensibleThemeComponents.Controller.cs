using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeComponents.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeComponents.Http;

[ApiController]
[Route("api/system-settings/extensible-theme")]
public class GetExtensibleThemeComponentsController(IGetExtensibleThemeComponentsService service) : ControllerBase
{
    [HttpGet("components/{themeId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ThemeComponents), 200)]
    public async Task<ActionResult<ThemeComponents>> GetThemeComponents(string themeId, CancellationToken cancellationToken)
    {
        var result = await service.GetExtensibleThemeComponentsAsync(themeId, cancellationToken);
        if (result == null) return NotFound($"Theme components not found for: {themeId}");
        return Ok(result);
    }
}
