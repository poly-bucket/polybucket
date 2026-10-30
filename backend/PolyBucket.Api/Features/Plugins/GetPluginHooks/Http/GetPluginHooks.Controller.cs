using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Plugins.GetPluginHooks.Domain;

namespace PolyBucket.Api.Features.Plugins.GetPluginHooks.Http;

[ApiController]
[Route("api/plugins")]
[Authorize]
[RequirePermission(PermissionConstants.PLUGIN_MANAGE)]
public class GetPluginHooksController(IGetPluginHooksService getPluginHooksService) : ControllerBase
{
    /// <summary>
    /// Get plugin hooks information for extension point management
    /// </summary>
    [HttpGet("hooks")]
    [ProducesResponseType(typeof(GetPluginHooksResponse), 200)]
    public ActionResult<GetPluginHooksResponse> GetPluginHooks()
    {
        return Ok(getPluginHooksService.GetHooks());
    }
}
