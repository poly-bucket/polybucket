using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.SetActiveExtensibleTheme.Domain;

namespace PolyBucket.Api.Features.SystemSettings.SetActiveExtensibleTheme.Http;

[ApiController]
[Route("api/system-settings/extensible-theme")]
public class SetActiveExtensibleThemeController(ISetActiveExtensibleThemeService service) : ControllerBase
{
    [HttpPost("active/{themeId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> SetActiveTheme(string themeId, CancellationToken cancellationToken)
    {
        var success = await service.SetActiveExtensibleThemeAsync(themeId, cancellationToken);
        if (!success) return BadRequest($"Failed to set theme: {themeId}");
        return Ok(new { message = $"Theme {themeId} activated successfully" });
    }
}
