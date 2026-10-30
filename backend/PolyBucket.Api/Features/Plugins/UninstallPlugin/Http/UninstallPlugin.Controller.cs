using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.UninstallPlugin.Domain;

namespace PolyBucket.Api.Features.Plugins.UninstallPlugin.Http;

[ApiController]
[Route("api/plugins")]
[Authorize]
public class UninstallPluginController(IUninstallPluginService uninstallPluginService) : ControllerBase
{
    private readonly IUninstallPluginService _uninstallPluginService = uninstallPluginService;

    /// <summary>
    /// Uninstall a plugin
    /// </summary>
    [HttpDelete("{pluginId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> UninstallPlugin(string pluginId, CancellationToken cancellationToken)
    {
        var outcome = await _uninstallPluginService.UninstallAsync(pluginId, cancellationToken);

        return outcome.Status switch
        {
            UninstallPluginStatus.BadRequest => BadRequest(new { message = "Plugin ID is required" }),
            UninstallPluginStatus.NotFound => NotFound(new { message = $"Plugin {pluginId} not found" }),
            _ => Ok(new { message = $"Plugin {pluginId} uninstalled successfully" })
        };
    }
}
