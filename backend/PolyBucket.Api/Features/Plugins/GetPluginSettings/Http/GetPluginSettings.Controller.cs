using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Plugins.GetPluginSettings.Domain;

namespace PolyBucket.Api.Features.Plugins.GetPluginSettings.Http;

[ApiController]
[Route("api/plugins")]
[Authorize]
[RequirePermission(PermissionConstants.PLUGIN_CONFIGURE)]
public class GetPluginSettingsController(IGetPluginSettingsService getPluginSettingsService) : ControllerBase
{
    /// <summary>
    /// Get current plugin configuration
    /// </summary>
    [HttpGet("{pluginId}/settings")]
    [ProducesResponseType(typeof(PluginSettingsResponse), 200)]
    [ProducesResponseType(404)]
    public ActionResult<PluginSettingsResponse> GetPluginSettings(string pluginId)
    {
        var response = getPluginSettingsService.GetSettings(pluginId);
        if (response == null)
        {
            return NotFound($"Plugin with ID '{pluginId}' not found");
        }

        return Ok(response);
    }
}
