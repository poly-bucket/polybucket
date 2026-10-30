using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.InstallMarketplacePlugin.Domain;

namespace PolyBucket.Api.Features.Plugins.InstallMarketplacePlugin.Http;

[ApiController]
[Route("api/plugins/marketplace")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Plugin Marketplace")]
public class InstallMarketplacePluginController(IInstallMarketplacePluginService service) : ControllerBase
{
    /// <summary>
    /// Install a plugin from the marketplace
    /// </summary>
    [HttpPost("{pluginId}/install")]
    [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
    [ProducesResponseType(typeof(PluginInstallationResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PluginInstallationResult>> InstallFromMarketplace(
        string pluginId,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var outcome = await service.InstallAsync(pluginId, userId, cancellationToken);

        return outcome.Status switch
        {
            InstallMarketplacePluginStatus.NotFound => NotFound(outcome.ErrorBody),
            InstallMarketplacePluginStatus.BadRequest when outcome.Result != null => BadRequest(outcome.Result),
            InstallMarketplacePluginStatus.BadRequest => BadRequest(outcome.ErrorBody),
            _ => Ok(outcome.Result)
        };
    }
}
