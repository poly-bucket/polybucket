using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Plugins.UpdatePluginSettings.Domain;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginSettings.Http;

[ApiController]
[Route("api/plugins")]
[Authorize]
[RequirePermission(PermissionConstants.PLUGIN_CONFIGURE)]
public class UpdatePluginSettingsController(IUpdatePluginSettingsService updatePluginSettingsService) : ControllerBase
{
    /// <summary>
    /// Update plugin settings
    /// </summary>
    [HttpPut("{pluginId}/settings")]
    [ProducesResponseType(typeof(UpdatePluginSettingsResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public ActionResult<UpdatePluginSettingsResponse> UpdatePluginSettings(
        string pluginId,
        [FromBody] UpdatePluginSettingsRequest request)
    {
        var response = updatePluginSettingsService.UpdateSettings(pluginId, request);
        if (response == null)
        {
            return NotFound($"Plugin with ID '{pluginId}' not found");
        }

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
