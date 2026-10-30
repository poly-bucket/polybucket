using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemePlugins.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemePlugins.Http;

[ApiController]
[Route("api/system-settings/extensible-theme")]
public class GetExtensibleThemePluginsController(IGetExtensibleThemePluginsService service) : ControllerBase
{
    [HttpGet("plugins")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<IThemePlugin>), 200)]
    public async Task<ActionResult<IEnumerable<IThemePlugin>>> GetThemePlugins(CancellationToken cancellationToken)
    {
        return Ok(await service.GetExtensibleThemePluginsAsync(cancellationToken));
    }
}
