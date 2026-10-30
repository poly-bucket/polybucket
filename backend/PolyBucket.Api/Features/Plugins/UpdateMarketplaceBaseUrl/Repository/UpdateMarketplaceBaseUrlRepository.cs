using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Plugins.GetMarketplaceBaseUrl.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Plugins.UpdateMarketplaceBaseUrl.Repository;

public class UpdateMarketplaceBaseUrlRepository(PolyBucketDbContext dbContext) : IUpdateMarketplaceBaseUrlRepository
{
    public async Task SaveBaseUrlAsync(string normalizedBaseUrl, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == MarketplaceSettingsKeys.MarketplaceBaseUrl, cancellationToken);

        if (entity == null)
        {
            entity = new Database.Entities.SystemSettings.SystemSetting
            {
                Key = MarketplaceSettingsKeys.MarketplaceBaseUrl,
                Value = normalizedBaseUrl
            };
            dbContext.SystemSettings.Add(entity);
        }
        else
        {
            entity.Value = normalizedBaseUrl;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
