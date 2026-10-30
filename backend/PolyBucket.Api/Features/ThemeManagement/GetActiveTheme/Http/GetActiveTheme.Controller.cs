using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ThemeManagement.GetActiveTheme.Domain;
using PolyBucket.Api.Features.ThemeManagement.GetActiveTheme.Http;

namespace PolyBucket.Api.Features.ThemeManagement.GetActiveTheme.Http;

[ApiController]
[Route("api/theme-management")]
public class GetActiveThemeController(IGetActiveThemeService service) : ControllerBase
{
    /// <summary>
    /// Get the currently active theme (public endpoint).
    /// </summary>
    [HttpGet("active-theme")]
    [ProducesResponseType(typeof(ThemeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThemeDto>> GetActiveTheme(CancellationToken cancellationToken)
    {
        var result = await service.GetActiveThemeAsync(cancellationToken);
        return result == null ? NotFound("No active theme found") : Ok(result);
    }
}
