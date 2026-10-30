using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Plugins.GetMarketplaceBaseUrl.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Plugins.GetMarketplaceBaseUrl.Repository;

public class GetMarketplaceBaseUrlRepository(PolyBucketDbContext dbContext) : IGetMarketplaceBaseUrlRepository
{
    public async Task<string?> GetBaseUrlAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SystemSettings
            .AsNoTracking()
            .Where(s => s.Key == MarketplaceSettingsKeys.MarketplaceBaseUrl)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(cancellationToken);
}
