using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetMarketplacePluginDetails.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePluginDetails.Http;

[ApiController]
[Route("api/plugins/marketplace")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Plugin Marketplace")]
public class GetMarketplacePluginDetailsController(IGetMarketplacePluginDetailsService service) : ControllerBase
{
    /// <summary>
    /// Get plugin details from the marketplace
    /// </summary>
    [HttpGet("{pluginId}")]
    [ProducesResponseType(typeof(MarketplacePluginDetails), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<MarketplacePluginDetails>> GetMarketplacePlugin(string pluginId, CancellationToken cancellationToken)
    {
        var plugin = await service.GetPluginDetailsAsync(pluginId, cancellationToken);
        if (plugin == null)
        {
            return NotFound(new { message = $"Plugin {pluginId} not found in marketplace" });
        }

        return Ok(plugin);
    }
}
