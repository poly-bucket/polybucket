using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Entities.Models.Enums;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public sealed class SiteModelUploadPolicy(
    PolyBucketDbContext context,
    IMemoryCache cache,
    ILogger<SiteModelUploadPolicy> logger) : ISiteModelUploadPolicy
{
    internal const string CacheKey = "site-model-upload-policy";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly PolyBucketDbContext _context = context;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<SiteModelUploadPolicy> _logger = logger;

    public async Task<bool> IsAnonymousModelUploadAllowedAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.AllowAnonymousModelUpload;
    }

    public async Task<bool> IsLoginRequiredForUnauthenticatedUploadAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.RequireLoginForUnauthenticated;
    }

    public async Task<PrivacySettings> GetDefaultModelPrivacyAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.DefaultModelPrivacy;
    }

    public void Invalidate()
    {
        _cache.Remove(CacheKey);
    }

    private async Task<SiteModelUploadPolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out SiteModelUploadPolicySnapshot? cached) && cached != null)
        {
            return cached;
        }

        var snapshot = await LoadSnapshotAsync(cancellationToken);
        _cache.Set(CacheKey, snapshot, CacheTtl);
        return snapshot;
    }

    private async Task<SiteModelUploadPolicySnapshot> LoadSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var systemSetup = await _context.SystemSetups
                .AsNoTracking()
                .Select(s => new { s.RequireLoginForUpload, s.DefaultModelPrivacy })
                .FirstOrDefaultAsync(cancellationToken);

            var modelSettings = await _context.ModelSettings
                .AsNoTracking()
                .Select(s => new { s.AllowAnonUploads })
                .FirstOrDefaultAsync(cancellationToken);

            var requireLoginForUpload = systemSetup?.RequireLoginForUpload ?? true;
            var allowAnon = modelSettings?.AllowAnonUploads ?? false;
            var defaultPrivacy = systemSetup?.DefaultModelPrivacy ?? PrivacySettings.Public;

            return new SiteModelUploadPolicySnapshot(requireLoginForUpload, allowAnon, defaultPrivacy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read site model upload policy; failing closed to login required");
            return SiteModelUploadPolicySnapshot.FailClosed;
        }
    }

    private sealed record SiteModelUploadPolicySnapshot(
        bool RequireLoginForUpload,
        bool AllowAnonUploads,
        PrivacySettings DefaultModelPrivacy)
    {
        public static SiteModelUploadPolicySnapshot FailClosed { get; } = new(true, false, PrivacySettings.Public);

        public bool AllowAnonymousModelUpload => !RequireLoginForUpload && AllowAnonUploads;

        public bool RequireLoginForUnauthenticated => RequireLoginForUpload || !AllowAnonUploads;
    }
}