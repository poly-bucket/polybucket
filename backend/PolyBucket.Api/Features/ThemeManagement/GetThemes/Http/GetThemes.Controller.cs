using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ThemeManagement.GetThemes.Domain;
using PolyBucket.Api.Features.ThemeManagement.GetThemes.Http;

namespace PolyBucket.Api.Features.ThemeManagement.GetThemes.Http;

[ApiController]
[Route("api/theme-management")]
[Authorize]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class GetThemesController(IGetThemesService service) : ControllerBase
{
    /// <summary>
    /// Get all themes and the active theme.
    /// </summary>
    [HttpGet("themes")]
    [ProducesResponseType(typeof(ThemeListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ThemeListResponse>> GetThemes(CancellationToken cancellationToken) =>
        Ok(await service.GetThemesAsync(cancellationToken));
}
