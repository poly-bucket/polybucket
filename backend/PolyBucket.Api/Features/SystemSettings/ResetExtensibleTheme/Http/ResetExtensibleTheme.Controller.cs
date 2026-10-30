using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.ResetExtensibleTheme.Domain;

namespace PolyBucket.Api.Features.SystemSettings.ResetExtensibleTheme.Http;

[ApiController]
[Route("api/system-settings/extensible-theme")]
public class ResetExtensibleThemeController(IResetExtensibleThemeService service) : ControllerBase
{
    [HttpPost("reset")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ThemeApplicationResult), 200)]
    public async Task<ActionResult<ThemeApplicationResult>> ResetToDefault(CancellationToken cancellationToken)
    {
        var result = await service.ResetExtensibleThemeAsync(cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
