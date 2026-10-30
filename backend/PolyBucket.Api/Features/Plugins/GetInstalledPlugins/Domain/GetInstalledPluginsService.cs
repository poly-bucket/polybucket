using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetInstalledPlugins.Repository;

namespace PolyBucket.Api.Features.Plugins.GetInstalledPlugins.Domain;

public class GetInstalledPluginsService(IGetInstalledPluginsRepository repository) : IGetInstalledPluginsService
{
    private readonly IGetInstalledPluginsRepository _repository = repository;

    public Task<List<InstalledPlugin>> GetInstalledPluginsAsync(CancellationToken cancellationToken = default) =>
        _repository.GetInstalledPluginsAsync(cancellationToken);
}
