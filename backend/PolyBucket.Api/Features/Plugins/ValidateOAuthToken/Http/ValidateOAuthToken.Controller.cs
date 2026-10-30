using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetOAuthUserInfo.Domain;
using PolyBucket.Api.Features.Plugins.ValidateOAuthToken.Domain;

namespace PolyBucket.Api.Features.Plugins.ValidateOAuthToken.Http;

[Authorize]
[ApiController]
[Route("api/plugins/oauth")]
public class ValidateOAuthTokenController(IValidateOAuthTokenService service) : ControllerBase
{
    [HttpPost("validate/{providerName}")]
    public async Task<ActionResult> ValidateToken(string providerName, [FromBody] OAuthValidateRequest request)
    {
        var outcome = await service.ValidateAsync(providerName, request);
        return outcome.Status switch
        {
            OAuthRequestStatus.NotFound => NotFound(outcome.ErrorBody),
            OAuthRequestStatus.BadRequest => BadRequest(outcome.ErrorBody),
            _ => Ok(new { valid = outcome.Value })
        };
    }
}
