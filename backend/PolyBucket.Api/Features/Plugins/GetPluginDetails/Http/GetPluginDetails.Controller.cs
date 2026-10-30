using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Plugins.GetPluginDetails.Domain;

namespace PolyBucket.Api.Features.Plugins.GetPluginDetails.Http;

[ApiController]
[Route("api/plugins")]
[Authorize]
[RequirePermission(PermissionConstants.PLUGIN_MANAGE)]
public class GetPluginDetailsController(IGetPluginDetailsService getPluginDetailsService) : ControllerBase
{
    /// <summary>
    /// Get detailed information about a specific plugin
    /// </summary>
    [HttpGet("{pluginId}/details")]
    [ProducesResponseType(typeof(GetPluginDetailsResponse), 200)]
    [ProducesResponseType(404)]
    public ActionResult<GetPluginDetailsResponse> GetPluginDetails(string pluginId)
    {
        var details = getPluginDetailsService.GetDetails(pluginId);
        if (details == null)
        {
            return NotFound($"Plugin with ID '{pluginId}' not found");
        }

        return Ok(details);
    }
}
