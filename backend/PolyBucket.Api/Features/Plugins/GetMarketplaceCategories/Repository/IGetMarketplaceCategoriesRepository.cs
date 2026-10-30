using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplaceCategories.Repository;

public interface IGetMarketplaceCategoriesRepository
{
    Task<List<MarketplaceCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
