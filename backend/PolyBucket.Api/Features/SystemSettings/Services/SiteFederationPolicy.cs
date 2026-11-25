using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public sealed class SiteFederationPolicy(
    PolyBucketDbContext context,
    IMemoryCache cache,
    ILogger<SiteFederationPolicy> logger) : ISiteFederationPolicy
{
    internal const string CacheKey = "site-federation-policy";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly PolyBucketDbContext _context = context;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<SiteFederationPolicy> _logger = logger;

    public async Task<SiteFederationPolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out SiteFederationPolicySnapshot? cached) && cached != null)
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

    private async Task<SiteFederationPolicySnapshot> LoadSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var setup = await _context.SystemSetups
                .AsNoTracking()
                .Select(s => new
                {
                    s.Id,
                    s.EnableFederation,
                    s.InstanceName,
                    s.InstanceDescription,
                    s.AdminContact
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (setup == null)
            {
                return SiteFederationPolicySnapshot.Disabled;
            }

            return new SiteFederationPolicySnapshot(
                setup.Id,
                setup.EnableFederation,
                setup.InstanceName ?? string.Empty,
                setup.InstanceDescription ?? string.Empty,
                setup.AdminContact ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read site federation policy; failing closed");
            return SiteFederationPolicySnapshot.Disabled;
        }
    }
}