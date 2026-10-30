using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplaceCategories.Domain;

public interface IGetMarketplaceCategoriesService
{
    Task<List<MarketplaceCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
