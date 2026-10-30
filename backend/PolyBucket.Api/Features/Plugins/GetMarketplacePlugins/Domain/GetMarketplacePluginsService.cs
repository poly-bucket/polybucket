using PolyBucket.Api.Features.Plugins.GetMarketplacePlugins.Repository;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePlugins.Domain;

public class GetMarketplacePluginsService(IGetMarketplacePluginsRepository repository) : IGetMarketplacePluginsService
{
    public Task<MarketplacePluginsResponse> GetPluginsAsync(int page, int pageSize, string? category, string? search, string? target, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        return repository.GetPluginsAsync(page, pageSize, category, search, target, cancellationToken);
    }
}
