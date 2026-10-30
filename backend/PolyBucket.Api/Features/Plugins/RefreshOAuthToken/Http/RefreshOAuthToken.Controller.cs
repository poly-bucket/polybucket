using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Domain;
using PolyBucket.Api.Features.Plugins.RefreshOAuthToken.Domain;

namespace PolyBucket.Api.Features.Plugins.RefreshOAuthToken.Http;

[Authorize]
[ApiController]
[Route("api/plugins/oauth")]
public class RefreshOAuthTokenController(IRefreshOAuthTokenService service) : ControllerBase
{
    [HttpPost("refresh/{providerName}")]
    [ProducesResponseType(typeof(OAuthTokenRefreshResult), 200)]
    public async Task<ActionResult<OAuthTokenRefreshResult>> RefreshToken(string providerName, [FromBody] OAuthRefreshRequest request)
    {
        var outcome = await service.RefreshAsync(providerName, request);
        return outcome.Status switch
        {
            OAuthRequestStatus.NotFound => NotFound(outcome.ErrorBody),
            OAuthRequestStatus.BadRequest => BadRequest(outcome.Value ?? outcome.ErrorBody),
            _ => Ok(outcome.Value)
        };
    }
}
