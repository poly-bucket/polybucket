using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplacePlugins.Domain;

public interface IGetMarketplacePluginsService
{
    Task<MarketplacePluginsResponse> GetPluginsAsync(int page, int pageSize, string? category, string? search, string? target, CancellationToken cancellationToken = default);
}
