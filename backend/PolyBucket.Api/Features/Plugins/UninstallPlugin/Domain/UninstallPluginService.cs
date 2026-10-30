using PolyBucket.Api.Features.Plugins.UninstallPlugin.Repository;

namespace PolyBucket.Api.Features.Plugins.UninstallPlugin.Domain;

public class UninstallPluginService(IUninstallPluginRepository repository) : IUninstallPluginService
{
    private readonly IUninstallPluginRepository _repository = repository;

    public UninstallPluginOutcome Uninstall(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
        {
            return new UninstallPluginOutcome { Status = UninstallPluginStatus.BadRequest, PluginId = pluginId };
        }

        return new UninstallPluginOutcome { Status = UninstallPluginStatus.Ok, PluginId = pluginId };
    }

    public async Task<UninstallPluginOutcome> UninstallAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        var validation = Uninstall(pluginId);
        if (validation.Status != UninstallPluginStatus.Ok)
        {
            return validation;
        }

        var success = await _repository.UninstallPluginAsync(pluginId, cancellationToken);
        return success
            ? new UninstallPluginOutcome { Status = UninstallPluginStatus.Ok, PluginId = pluginId }
            : new UninstallPluginOutcome { Status = UninstallPluginStatus.NotFound, PluginId = pluginId };
    }
}
