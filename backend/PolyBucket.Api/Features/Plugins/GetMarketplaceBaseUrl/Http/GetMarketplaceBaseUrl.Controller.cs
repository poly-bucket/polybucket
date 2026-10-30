using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Plugins.GetMarketplaceBaseUrl.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMarketplaceBaseUrl.Http;

[ApiController]
[Route("api/admin/marketplace")]
[Authorize]
public class GetMarketplaceBaseUrlController(IGetMarketplaceBaseUrlService service) : ControllerBase
{
    /// <summary>
    /// Get the configured marketplace base URL.
    /// </summary>
    [HttpGet("base-url")]
    [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
    [ProducesResponseType(typeof(MarketplaceBaseUrlResponse), 200)]
    public async Task<ActionResult<MarketplaceBaseUrlResponse>> GetMarketplaceBaseUrl(CancellationToken cancellationToken)
    {
        var baseUrl = await service.GetBaseUrlAsync(cancellationToken);
        return Ok(new MarketplaceBaseUrlResponse { BaseUrl = baseUrl });
    }
}
