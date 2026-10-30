using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.GetTokenSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.SystemSettings.GetTokenSettings.Http;

[ApiController]
[Route("api/system-settings/token")]
[Authorize(Roles = "Admin")]
public class GetTokenSettingsController(IGetTokenSettingsService getTokenSettingsService) : ControllerBase
{
    /// <summary>
    /// Get the current token settings
    /// </summary>
    /// <returns>Current token configuration</returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(TokenSettings))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<TokenSettings>> GetTokenSettings(CancellationToken cancellationToken)
    {
        return Ok(await getTokenSettingsService.GetTokenSettingsAsync(cancellationToken));
    }
}
