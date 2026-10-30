using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePlugins.Repository;

public interface IGetMarketplacePluginsRepository
{
    Task<MarketplacePluginsResponse> GetPluginsAsync(int page, int pageSize, string? category, string? search, string? target, CancellationToken cancellationToken = default);
}
