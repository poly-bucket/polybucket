using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public class SiteBrandingState(
    PolyBucketDbContext context,
    IMemoryCache cache,
    ILogger<SiteBrandingState> logger) : ISiteBrandingState
{
    internal const string CacheKey = "site-branding:site-name";
    private const string DefaultSiteName = "PolyBucket";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly PolyBucketDbContext _context = context;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<SiteBrandingState> _logger = logger;

    public async Task<string> GetSiteNameAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out string? cached) && !string.IsNullOrWhiteSpace(cached))
        {
            return cached;
        }

        string siteName;
        try
        {
            var name = await _context.SystemSetups
                .AsNoTracking()
                .Select(s => s.SiteName)
                .FirstOrDefaultAsync(cancellationToken);

            siteName = string.IsNullOrWhiteSpace(name) ? DefaultSiteName : name;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read site name; using default");
            return DefaultSiteName;
        }

        _cache.Set(CacheKey, siteName, CacheTtl);
        return siteName;
    }

    public void Invalidate()
    {
        _cache.Remove(CacheKey);
    }
}