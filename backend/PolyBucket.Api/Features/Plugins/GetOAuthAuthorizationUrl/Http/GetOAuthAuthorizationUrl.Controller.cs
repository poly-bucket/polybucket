using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetOAuthAuthorizationUrl.Domain;

namespace PolyBucket.Api.Features.Plugins.GetOAuthAuthorizationUrl.Http;

[Authorize]
[ApiController]
[Route("api/plugins/oauth")]
public class GetOAuthAuthorizationUrlController(IGetOAuthAuthorizationUrlService service) : ControllerBase
{
    /// <summary>
    /// Get OAuth authorization URL for a provider
    /// </summary>
    [HttpGet("authorize/{providerName}")]
    [ProducesResponseType(typeof(OAuthAuthorizationUrlResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public ActionResult GetAuthorizationUrl(string providerName, [FromQuery] string redirectUri, [FromQuery] string? state = null)
    {
        var outcome = service.GetAuthorizationUrl(providerName, redirectUri, state);
        return outcome.Status switch
        {
            OAuthAuthorizationUrlStatus.NotFound => NotFound(outcome.ErrorBody),
            OAuthAuthorizationUrlStatus.BadRequest => BadRequest(outcome.ErrorBody),
            _ => Ok(outcome.Response)
        };
    }
}
