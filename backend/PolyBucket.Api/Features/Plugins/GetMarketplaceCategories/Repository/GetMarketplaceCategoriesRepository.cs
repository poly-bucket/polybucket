using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.GetMarketplaceCategories.Repository;

public class GetMarketplaceCategoriesRepository(MarketplaceClient marketplaceClient) : IGetMarketplaceCategoriesRepository
{
    public Task<List<MarketplaceCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        marketplaceClient.GetCategoriesAsync();
}
