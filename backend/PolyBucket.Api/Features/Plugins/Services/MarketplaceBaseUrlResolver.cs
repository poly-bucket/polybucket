using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Plugins.GetMarketplaceBaseUrl.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Plugins.Services;

public class MarketplaceBaseUrlResolver(
    PolyBucketDbContext dbContext,
    IConfiguration configuration) : IMarketplaceBaseUrlResolver
{
    public async Task<string> ResolveAsync(CancellationToken cancellationToken = default)
    {
        var stored = await dbContext.SystemSettings
            .AsNoTracking()
            .Where(s => s.Key == MarketplaceSettingsKeys.MarketplaceBaseUrl)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(stored))
        {
            return stored.TrimEnd('/');
        }

        var configured = configuration["Marketplace:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.TrimEnd('/');
        }

        return "http://localhost:5001";
    }
}
