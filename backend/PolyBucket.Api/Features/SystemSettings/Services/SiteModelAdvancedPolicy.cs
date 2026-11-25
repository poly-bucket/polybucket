using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public sealed class SiteModelAdvancedPolicy(
    PolyBucketDbContext context,
    IMemoryCache cache,
    ILogger<SiteModelAdvancedPolicy> logger) : ISiteModelAdvancedPolicy
{
    internal const string CacheKey = "site-model-advanced-policy";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly PolyBucketDbContext _context = context;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<SiteModelAdvancedPolicy> _logger = logger;

    public async Task<SiteModelAdvancedPolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out SiteModelAdvancedPolicySnapshot? cached) && cached != null)
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

    private async Task<SiteModelAdvancedPolicySnapshot> LoadSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _context.ModelSettings
                .AsNoTracking()
                .Select(s => new
                {
                    s.EnableModelImport,
                    s.EnableModelAnalytics,
                    s.EnableModelReporting,
                    s.RequireReportDetails,
                    s.EnableModelRejection,
                    s.RequireRejectionReason,
                    s.EnableModelDeletion,
                    s.EnableModelSharing
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (settings == null)
            {
                return SiteModelAdvancedPolicySnapshot.FailClosed;
            }

            return new SiteModelAdvancedPolicySnapshot(
                settings.EnableModelImport,
                settings.EnableModelAnalytics,
                settings.EnableModelReporting,
                settings.RequireReportDetails,
                settings.EnableModelRejection,
                settings.RequireRejectionReason,
                settings.EnableModelDeletion,
                settings.EnableModelSharing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read site model advanced policy; failing closed");
            return SiteModelAdvancedPolicySnapshot.FailClosed;
        }
    }
}