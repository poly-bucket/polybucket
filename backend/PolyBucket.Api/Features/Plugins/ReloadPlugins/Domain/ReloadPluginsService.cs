using PolyBucket.Api.Features.Plugins.ReloadPlugins.Repository;

namespace PolyBucket.Api.Features.Plugins.ReloadPlugins.Domain;

public class ReloadPluginsService(IReloadPluginsRepository repository) : IReloadPluginsService
{
    private readonly IReloadPluginsRepository _repository = repository;

    public Task ReloadAsync(CancellationToken cancellationToken = default) =>
        _repository.ReloadAsync(cancellationToken);
}
