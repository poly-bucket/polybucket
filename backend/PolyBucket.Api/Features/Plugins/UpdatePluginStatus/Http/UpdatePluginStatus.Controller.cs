using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Plugins.UpdatePluginStatus.Domain;
using PolyBucket.Api.Features.Plugins.UpdatePluginStatus.Http;

namespace PolyBucket.Api.Features.Plugins.UpdatePluginStatus.Http;

[ApiController]
[Route("api/plugins")]
[Authorize]
[RequirePermission(PermissionConstants.PLUGIN_CONFIGURE)]
public class UpdatePluginStatusController(IUpdatePluginStatusService updatePluginStatusService) : ControllerBase
{
    /// <summary>
    /// Enable or disable a plugin
    /// </summary>
    [HttpPut("{pluginId}/status")]
    [ProducesResponseType(typeof(PluginStatusResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public ActionResult<PluginStatusResponse> UpdatePluginStatus(
        string pluginId,
        [FromBody] UpdatePluginStatusRequest request)
    {
        var response = updatePluginStatusService.UpdateStatus(pluginId, request);
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
