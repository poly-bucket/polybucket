using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Plugins.GetPluginsOverview.Domain;

namespace PolyBucket.Api.Features.Plugins.GetPluginsOverview.Http;

[ApiController]
[Route("api/plugins")]
[Authorize]
[RequirePermission(PermissionConstants.PLUGIN_MANAGE)]
public class GetPluginsOverviewController(IGetPluginsOverviewService getPluginsOverviewService) : ControllerBase
{
    /// <summary>
    /// Get all plugins with their basic information and status
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(GetPluginsOverviewResponse), 200)]
    public ActionResult<GetPluginsOverviewResponse> GetPluginsOverview()
    {
        return Ok(getPluginsOverviewService.GetOverview());
    }
}
