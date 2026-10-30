namespace PolyBucket.Api.Features.Plugins.UninstallPlugin.Repository;

public interface IUninstallPluginRepository
{
    Task<bool> UninstallPluginAsync(string pluginId, CancellationToken cancellationToken = default);
}
