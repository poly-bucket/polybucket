using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeConfiguration.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeConfiguration.Http;

[ApiController]
[Route("api/system-settings/extensible-theme")]
public class GetExtensibleThemeConfigurationController(IGetExtensibleThemeConfigurationService service) : ControllerBase
{
    [HttpGet("configuration")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
    public async Task<ActionResult<Dictionary<string, object>>> GetCurrentConfiguration(CancellationToken cancellationToken)
    {
        return Ok(await service.GetExtensibleThemeConfigurationAsync(cancellationToken));
    }
}
