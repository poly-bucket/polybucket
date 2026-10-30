using PolyBucket.Api.Features.Plugins.GetPlugins.Repository;

namespace PolyBucket.Api.Features.Plugins.GetPlugins.Domain;

public class GetPluginsService(IGetPluginsRepository repository) : IGetPluginsService
{
    private readonly IGetPluginsRepository _repository = repository;

    public IReadOnlyList<GetPluginsItem> GetPlugins() => _repository.GetLoadedPlugins();
}
