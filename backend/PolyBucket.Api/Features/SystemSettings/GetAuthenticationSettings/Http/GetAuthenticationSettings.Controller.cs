using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Http;

[ApiController]
[Route("api/system-settings/auth")]
[Authorize(Roles = "Admin")]
public class GetAuthenticationSettingsController(IGetAuthenticationSettingsService getAuthenticationSettingsService) : ControllerBase
{
    /// <summary>
    /// Get the current authentication settings
    /// </summary>
    /// <returns>Current authentication configuration</returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(AuthenticationSettings))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<AuthenticationSettings>> GetAuthenticationSettings(CancellationToken cancellationToken)
    {
        return Ok(await getAuthenticationSettingsService.GetAuthenticationSettingsAsync(cancellationToken));
    }
}
