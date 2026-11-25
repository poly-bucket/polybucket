using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public sealed class SiteModelVersioningPolicy(
    PolyBucketDbContext context,
    IMemoryCache cache,
    IPermissionService permissionService,
    ILogger<SiteModelVersioningPolicy> logger) : ISiteModelVersioningPolicy
{
    internal const string CacheKey = "site-model-versioning-policy";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly PolyBucketDbContext _context = context;
    private readonly IMemoryCache _cache = cache;
    private readonly IPermissionService _permissionService = permissionService;
    private readonly ILogger<SiteModelVersioningPolicy> _logger = logger;

    public async Task<bool> IsModelVersioningEnabledAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.EnableModelVersioning;
    }

    public async Task<SiteModelVersioningPolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out SiteModelVersioningPolicySnapshot? cached) && cached != null)
        {
            return cached;
        }

        var snapshot = await LoadSnapshotAsync(cancellationToken);
        _cache.Set(CacheKey, snapshot, CacheTtl);
        return snapshot;
    }

    public async Task<bool> CanViewVersionHistoryWhenDisabledAsync(
        ClaimsPrincipal user,
        Guid modelAuthorId,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = user.FindUserIdClaim();
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            if (modelAuthorId == userId)
            {
                return true;
            }

            if (await _permissionService.IsAdminAsync(userId))
            {
                return true;
            }

            var userRole = await _permissionService.GetUserRoleAsync(userId);
            if (userRole?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
        }

        return false;
    }

    public void Invalidate()
    {
        _cache.Remove(CacheKey);
    }

    private async Task<SiteModelVersioningPolicySnapshot> LoadSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var modelSettings = await _context.ModelSettings
                .AsNoTracking()
                .Select(s => new { s.EnableModelVersioning })
                .FirstOrDefaultAsync(cancellationToken);

            return new SiteModelVersioningPolicySnapshot(modelSettings?.EnableModelVersioning ?? false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read site model versioning policy; failing closed");
            return SiteModelVersioningPolicySnapshot.FailClosed;
        }
    }
}