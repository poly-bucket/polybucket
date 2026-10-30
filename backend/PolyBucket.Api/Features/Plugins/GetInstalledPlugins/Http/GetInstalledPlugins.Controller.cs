using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetInstalledPlugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetInstalledPlugins.Http;

[ApiController]
[Route("api/plugins")]
[Authorize]
public class GetInstalledPluginsController(IGetInstalledPluginsService getInstalledPluginsService) : ControllerBase
{
    private readonly IGetInstalledPluginsService _getInstalledPluginsService = getInstalledPluginsService;

    /// <summary>
    /// Get all installed plugins
    /// </summary>
    [HttpGet("installed")]
    [ProducesResponseType(typeof(List<InstalledPlugin>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<InstalledPlugin>>> GetInstalledPlugins(CancellationToken cancellationToken)
    {
        var plugins = await _getInstalledPluginsService.GetInstalledPluginsAsync(cancellationToken);
        return Ok(plugins);
    }
}
