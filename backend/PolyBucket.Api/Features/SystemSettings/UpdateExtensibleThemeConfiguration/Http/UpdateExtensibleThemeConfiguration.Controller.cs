using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.UpdateExtensibleThemeConfiguration.Domain;

namespace PolyBucket.Api.Features.SystemSettings.UpdateExtensibleThemeConfiguration.Http;

[ApiController]
[Route("api/system-settings/extensible-theme")]
public class UpdateExtensibleThemeConfigurationController(IUpdateExtensibleThemeConfigurationService service) : ControllerBase
{
    [HttpPut("configuration")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ThemeApplicationResult), 200)]
    public async Task<ActionResult<ThemeApplicationResult>> UpdateConfiguration([FromBody] Dictionary<string, object> configuration, CancellationToken cancellationToken)
    {
        if (configuration == null) return BadRequest("Configuration is required");
        var result = await service.UpdateExtensibleThemeConfigurationAsync(configuration, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
