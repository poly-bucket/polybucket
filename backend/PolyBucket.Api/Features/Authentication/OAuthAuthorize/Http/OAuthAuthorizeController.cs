using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Services;

namespace PolyBucket.Api.Features.Authentication.OAuthAuthorize.Http;

[ApiController]
[Route("api/auth/oauth")]
public class OAuthAuthorizeController(
    IOAuthService oauthService,
    ILogger<OAuthAuthorizeController> logger) : ControllerBase
{
    /// <summary>
    /// Build the provider authorization URL for the OAuth redirect flow.
    /// </summary>
    [HttpGet("{provider}/authorize")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetAuthorizationUrl(string provider, [FromQuery] string redirectUri, [FromQuery] string? state = null)
    {
        try
        {
            var stateParam = state ?? Guid.NewGuid().ToString();
            var authUrl = await oauthService.GetAuthorizationUrlAsync(provider, redirectUri, stateParam);
            return Ok(new { authorizationUrl = authUrl, state = stateParam });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting authorization URL for provider {Provider}", provider);
            return BadRequest(new { message = "Invalid OAuth provider or configuration" });
        }
    }
}
