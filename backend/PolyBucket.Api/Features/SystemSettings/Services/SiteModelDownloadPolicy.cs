using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public sealed class SiteModelDownloadPolicy(
    PolyBucketDbContext context,
    IMemoryCache cache,
    ILogger<SiteModelDownloadPolicy> logger) : ISiteModelDownloadPolicy
{
    internal const string CacheKey = "site-model-download-policy";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly PolyBucketDbContext _context = context;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<SiteModelDownloadPolicy> _logger = logger;

    public async Task<bool> IsAnonymousModelDownloadAllowedAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.AllowAnonDownloads;
    }

    public async Task<bool> AreModelDownloadsEnabledAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.EnableModelDownloads;
    }

    public async Task<SiteModelDownloadPolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out SiteModelDownloadPolicySnapshot? cached) && cached != null)
        {
            return cached;
        }

        var snapshot = await LoadSnapshotAsync(cancellationToken);
        _cache.Set(CacheKey, snapshot, CacheTtl);
        return snapshot;
    }

    public void Invalidate()
    {
        _cache.Remove(CacheKey);
    }

    private async Task<SiteModelDownloadPolicySnapshot> LoadSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var modelSettings = await _context.ModelSettings
                .AsNoTracking()
                .Select(s => new { s.AllowAnonDownloads, s.EnableModelDownloads })
                .FirstOrDefaultAsync(cancellationToken);

            return new SiteModelDownloadPolicySnapshot(
                modelSettings?.AllowAnonDownloads ?? false,
                modelSettings?.EnableModelDownloads ?? false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read site model download policy; failing closed");
            return SiteModelDownloadPolicySnapshot.FailClosed;
        }
    }
}