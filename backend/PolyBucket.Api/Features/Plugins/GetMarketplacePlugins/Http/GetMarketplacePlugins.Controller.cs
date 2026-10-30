using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetMarketplacePlugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePlugins.Http;

[ApiController]
[Route("api/plugins/marketplace")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Plugin Marketplace")]
public class GetMarketplacePluginsController(IGetMarketplacePluginsService getMarketplacePluginsService) : ControllerBase
{
    /// <summary>
    /// Get available plugins from the marketplace
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(MarketplacePluginsResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<MarketplacePluginsResponse>> GetMarketplacePlugins(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] string? target = null,
        CancellationToken cancellationToken = default)
    {
        var response = await getMarketplacePluginsService.GetPluginsAsync(page, pageSize, category, search, target, cancellationToken);
        return Ok(response);
    }
}
