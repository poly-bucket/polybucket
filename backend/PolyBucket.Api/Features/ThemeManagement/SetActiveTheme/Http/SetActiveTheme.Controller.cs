using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ThemeManagement.SetActiveTheme.Domain;
using PolyBucket.Api.Features.ThemeManagement.SetActiveTheme.Http;

namespace PolyBucket.Api.Features.ThemeManagement.SetActiveTheme.Http;

[ApiController]
[Route("api/theme-management")]
[Authorize]
public class SetActiveThemeController(ISetActiveThemeService service) : ControllerBase
{
    /// <summary>
    /// Set a theme as active.
    /// </summary>
    [HttpPost("themes/{themeId:int}/activate")]
    [ProducesResponseType(typeof(SetActiveThemeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SetActiveThemeResponse>> SetActiveTheme(int themeId, CancellationToken cancellationToken)
    {
        var result = await service.SetActiveThemeAsync(themeId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
