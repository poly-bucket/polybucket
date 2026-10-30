namespace PolyBucket.Api.Features.Plugins.ReloadPlugins.Domain;

public interface IReloadPluginsService
{
    Task ReloadAsync(CancellationToken cancellationToken = default);
}
