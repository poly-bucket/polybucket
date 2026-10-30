using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemes.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemes.Http;

[ApiController]
[Route("api/system-settings/extensible-theme")]
public class GetExtensibleThemesController(IGetExtensibleThemesService service) : ControllerBase
{
    [HttpGet("themes")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ThemeDefinition>), 200)]
    public async Task<ActionResult<IEnumerable<ThemeDefinition>>> GetAvailableThemes(CancellationToken cancellationToken)
    {
        return Ok(await service.GetExtensibleThemesAsync(cancellationToken));
    }
}
