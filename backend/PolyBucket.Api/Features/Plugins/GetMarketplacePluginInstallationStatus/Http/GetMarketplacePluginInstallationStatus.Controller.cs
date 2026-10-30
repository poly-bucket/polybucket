using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetMarketplacePluginInstallationStatus.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePluginInstallationStatus.Http;

[ApiController]
[Route("api/plugins/marketplace")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Plugin Marketplace")]
public class GetMarketplacePluginInstallationStatusController(IGetMarketplacePluginInstallationStatusService service) : ControllerBase
{
    /// <summary>
    /// Get plugin installation status
    /// </summary>
    [HttpGet("{pluginId}/status")]
    [ProducesResponseType(typeof(GetMarketplacePluginInstallationStatusResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<GetMarketplacePluginInstallationStatusResponse>> GetPluginInstallationStatus(
        string pluginId,
        CancellationToken cancellationToken)
    {
        var status = await service.GetStatusAsync(pluginId, cancellationToken);
        return Ok(status);
    }
}
