using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.HandleOAuthCallback.Domain;

namespace PolyBucket.Api.Features.Plugins.HandleOAuthCallback.Http;

[Authorize]
[ApiController]
[Route("api/plugins/oauth")]
public class HandleOAuthCallbackController(IHandleOAuthCallbackService service) : ControllerBase
{
    /// <summary>
    /// Handle OAuth callback and exchange code for tokens
    /// </summary>
    [HttpPost("callback/{providerName}")]
    [ProducesResponseType(typeof(OAuthAuthorizationResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OAuthAuthorizationResult>> HandleCallback(string providerName, [FromBody] OAuthCallbackRequest request)
    {
        var outcome = await service.HandleAsync(providerName, request);
        return outcome.Status switch
        {
            HandleOAuthCallbackStatus.NotFound => NotFound(outcome.ErrorBody),
            HandleOAuthCallbackStatus.BadRequest => BadRequest(outcome.Result ?? outcome.ErrorBody),
            _ => Ok(outcome.Result)
        };
    }
}
