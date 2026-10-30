using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.SystemSettings.ValidateExtensibleThemeConfiguration.Domain;

namespace PolyBucket.Api.Features.SystemSettings.ValidateExtensibleThemeConfiguration.Http;

[ApiController]
[Route("api/system-settings/extensible-theme")]
public class ValidateExtensibleThemeConfigurationController(IValidateExtensibleThemeConfigurationService service) : ControllerBase
{
    [HttpPost("validate/{themeId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ThemeValidationResult), 200)]
    public async Task<ActionResult<ThemeValidationResult>> ValidateConfiguration(string themeId, [FromBody] Dictionary<string, object> configuration, CancellationToken cancellationToken)
    {
        if (configuration == null) return BadRequest("Configuration is required");
        return Ok(await service.ValidateExtensibleThemeConfigurationAsync(themeId, configuration, cancellationToken));
    }
}
