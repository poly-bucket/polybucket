using PolyBucket.Api.Features.Plugins.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetOAuthProviders.Domain;

namespace PolyBucket.Api.Features.Plugins.GetOAuthProviders.Http;

[Authorize]
[ApiController]
[Route("api/plugins/oauth")]
public class GetOAuthProvidersController(IGetOAuthProvidersService service) : ControllerBase
{
    /// <summary>
    /// Get all registered OAuth providers
    /// </summary>
    [HttpGet("providers")]
    [ProducesResponseType(typeof(List<OAuthProviderInfo>), 200)]
    public ActionResult<List<OAuthProviderInfo>> GetOAuthProviders()
    {
        return Ok(service.GetProviders());
    }
}
