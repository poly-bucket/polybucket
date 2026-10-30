using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetMarketplaceCategories.Domain;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplaceCategories.Http;

[ApiController]
[Route("api/plugins/marketplace")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Plugin Marketplace")]
public class GetMarketplaceCategoriesController(IGetMarketplaceCategoriesService service) : ControllerBase
{
    /// <summary>
    /// Get plugin categories from the marketplace
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<MarketplaceCategory>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<MarketplaceCategory>>> GetMarketplaceCategories(CancellationToken cancellationToken)
    {
        var categories = await service.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }
}
