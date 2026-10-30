using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.ReloadPlugins.Domain;

namespace PolyBucket.Api.Features.Plugins.ReloadPlugins.Http;

[ApiController]
[Route("api/plugins")]
[Authorize(Roles = "Admin")]
public class ReloadPluginsController(IReloadPluginsService reloadPluginsService) : ControllerBase
{
    private readonly IReloadPluginsService _reloadPluginsService = reloadPluginsService;

    /// <summary>
    /// Reload all plugins from disk.
    /// </summary>
    [HttpPost("reload")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ReloadPlugins(CancellationToken cancellationToken)
    {
        await _reloadPluginsService.ReloadAsync(cancellationToken);
        return Ok();
    }
}
