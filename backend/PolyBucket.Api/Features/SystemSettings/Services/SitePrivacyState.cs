using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public class SitePrivacyState(
    PolyBucketDbContext context,
    IMemoryCache cache,
    ILogger<SitePrivacyState> logger) : ISitePrivacyState
{
    internal const string CacheKey = "site-privacy:allow-public-browsing";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly PolyBucketDbContext _context = context;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<SitePrivacyState> _logger = logger;

    public async Task<bool> IsPublicBrowsingAllowedAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out bool cached))
        {
            return cached;
        }

        bool allowPublicBrowsing;
        try
        {
            var setup = await _context.SystemSetups
                .AsNoTracking()
                .Select(s => new { s.AllowPublicBrowsing })
                .FirstOrDefaultAsync(cancellationToken);

            allowPublicBrowsing = setup?.AllowPublicBrowsing ?? true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read site privacy state; failing open to public browsing");
            return true;
        }

        _cache.Set(CacheKey, allowPublicBrowsing, CacheTtl);
        return allowPublicBrowsing;
    }

    public void Invalidate()
    {
        _cache.Remove(CacheKey);
    }
}
