using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.InstallMarketplacePlugin.Repository;

namespace PolyBucket.Api.Features.Plugins.InstallMarketplacePlugin.Domain;

public class InstallMarketplacePluginService(IInstallMarketplacePluginRepository repository) : IInstallMarketplacePluginService
{
    public async Task<InstallMarketplacePluginOutcome> InstallAsync(string pluginId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
        {
            return new InstallMarketplacePluginOutcome
            {
                Status = InstallMarketplacePluginStatus.BadRequest,
                ErrorBody = new { message = "Plugin ID is required" }
            };
        }

        var pluginDetails = await repository.GetPluginDetailsAsync(pluginId, cancellationToken);
        if (pluginDetails == null)
        {
            return new InstallMarketplacePluginOutcome
            {
                Status = InstallMarketplacePluginStatus.NotFound,
                ErrorBody = new { message = $"Plugin {pluginId} not found in marketplace" }
            };
        }

        var result = await repository.InstallFromMarketplaceAsync(pluginDetails, cancellationToken);
        if (!result.Success)
        {
            return new InstallMarketplacePluginOutcome { Status = InstallMarketplacePluginStatus.BadRequest, Result = result };
        }

        await repository.RecordInstallationAsync(pluginId, result.Version, userId, cancellationToken);
        return new InstallMarketplacePluginOutcome { Status = InstallMarketplacePluginStatus.Ok, Result = result };
    }
}
