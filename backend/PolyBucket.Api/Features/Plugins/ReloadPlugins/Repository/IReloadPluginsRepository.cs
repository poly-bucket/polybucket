namespace PolyBucket.Api.Features.Plugins.ReloadPlugins.Repository;

public interface IReloadPluginsRepository
{
    Task ReloadAsync(CancellationToken cancellationToken = default);
}
