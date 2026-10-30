using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Plugins.GetMarketplaceBaseUrl.Http;
using PolyBucket.Api.Features.Plugins.UpdateMarketplaceBaseUrl.Domain;

namespace PolyBucket.Api.Features.Plugins.UpdateMarketplaceBaseUrl.Http;

[ApiController]
[Route("api/admin/marketplace")]
[Authorize]
public class UpdateMarketplaceBaseUrlController(IUpdateMarketplaceBaseUrlService service) : ControllerBase
{
    /// <summary>
    /// Save the marketplace base URL used for plugin catalog browsing and install.
    /// </summary>
    [HttpPut("base-url")]
    [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
    [ProducesResponseType(typeof(MarketplaceBaseUrlResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<MarketplaceBaseUrlResponse>> UpdateMarketplaceBaseUrl(
        [FromBody] UpdateMarketplaceBaseUrlRequest request,
        CancellationToken cancellationToken)
    {
        var validation = service.ValidateAndNormalize(request.BaseUrl);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = validation.ErrorMessage });
        }

        await service.SaveAsync(validation.NormalizedBaseUrl, cancellationToken);
        return Ok(new MarketplaceBaseUrlResponse { BaseUrl = validation.NormalizedBaseUrl });
    }
}
