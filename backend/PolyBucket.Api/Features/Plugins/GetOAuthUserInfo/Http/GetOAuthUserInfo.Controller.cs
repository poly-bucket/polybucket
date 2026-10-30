using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Domain;

namespace PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Http;

[Authorize]
[ApiController]
[Route("api/plugins/oauth")]
public class GetOAuthUserInfoController(IGetOAuthUserInfoService service) : ControllerBase
{
    [HttpGet("userinfo/{providerName}")]
    [ProducesResponseType(typeof(OAuthUserInfo), 200)]
    public async Task<ActionResult<OAuthUserInfo>> GetUserInfo(string providerName, [FromQuery] string accessToken)
    {
        var outcome = await service.GetUserInfoAsync(providerName, accessToken);
        return outcome.Status switch
        {
            OAuthRequestStatus.NotFound => NotFound(outcome.ErrorBody),
            OAuthRequestStatus.BadRequest => BadRequest(outcome.ErrorBody),
            _ => Ok(outcome.Value)
        };
    }
}
