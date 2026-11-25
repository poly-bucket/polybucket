using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public sealed class SiteModelFeaturePolicy(
    PolyBucketDbContext context,
    IMemoryCache cache,
    IPermissionService permissionService,
    ILogger<SiteModelFeaturePolicy> logger) : ISiteModelFeaturePolicy
{
    internal const string CacheKey = "site-model-feature-policy";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly PolyBucketDbContext _context = context;
    private readonly IMemoryCache _cache = cache;
    private readonly IPermissionService _permissionService = permissionService;
    private readonly ILogger<SiteModelFeaturePolicy> _logger = logger;

    public async Task<FeatureAccessLevel> GetCommentsAccessLevelAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.CommentsAccess;
    }

    public async Task<FeatureAccessLevel> GetCollectionsAccessLevelAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.CollectionsAccess;
    }

    public Task<bool> CanReadCommentsAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
        CanReadAsync(user, () => GetCommentsAccessLevelAsync(cancellationToken), cancellationToken);

    public Task<bool> CanWriteCommentsAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
        CanWriteAsync(user, () => GetCommentsAccessLevelAsync(cancellationToken), cancellationToken);

    public Task<bool> CanReadCollectionsAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
        CanReadAsync(user, () => GetCollectionsAccessLevelAsync(cancellationToken), cancellationToken);

    public Task<bool> CanWriteCollectionsAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
        CanWriteAsync(user, () => GetCollectionsAccessLevelAsync(cancellationToken), cancellationToken);

    public async Task<ModelCreationQuotaOutcome> ValidateModelCreationQuotaAsync(
        Guid authorId,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);

        if (snapshot.LimitTotalModels > 0)
        {
            var siteCount = await _context.Models
                .AsNoTracking()
                .CountAsync(m => m.DeletedAt == null, cancellationToken);
            if (siteCount >= snapshot.LimitTotalModels)
            {
                return ModelCreationQuotaOutcome.SiteLimit();
            }
        }

        if (snapshot.MaxModelsPerUser > 0)
        {
            var userCount = await _context.Models
                .AsNoTracking()
                .CountAsync(m => m.DeletedAt == null && m.AuthorId == authorId, cancellationToken);
            if (userCount >= snapshot.MaxModelsPerUser)
            {
                return ModelCreationQuotaOutcome.UserLimit();
            }
        }

        return ModelCreationQuotaOutcome.Allowed;
    }

    public void Invalidate()
    {
        _cache.Remove(CacheKey);
    }

    private async Task<bool> CanReadAsync(
        ClaimsPrincipal user,
        Func<Task<FeatureAccessLevel>> accessLevel,
        CancellationToken cancellationToken)
    {
        var level = await accessLevel();
        return level switch
        {
            FeatureAccessLevel.Full => true,
            FeatureAccessLevel.ReadOnly => true,
            _ => false
        };
    }

    private async Task<bool> CanWriteAsync(
        ClaimsPrincipal user,
        Func<Task<FeatureAccessLevel>> accessLevel,
        CancellationToken cancellationToken)
    {
        var level = await accessLevel();
        return level switch
        {
            FeatureAccessLevel.Full => true,
            FeatureAccessLevel.ReadOnly => await IsAdminOrModeratorAsync(user, cancellationToken),
            _ => false
        };
    }

    private async Task<bool> IsAdminOrModeratorAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return false;
        }

        if (await _permissionService.IsAdminAsync(userId))
        {
            return true;
        }

        var userRole = await _permissionService.GetUserRoleAsync(userId);
        return userRole?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true;
    }

    private async Task<SiteModelFeaturePolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out SiteModelFeaturePolicySnapshot? cached) && cached != null)
        {
            return cached;
        }

        var snapshot = await LoadSnapshotAsync(cancellationToken);
        _cache.Set(CacheKey, snapshot, CacheTtl);
        return snapshot;
    }

    private async Task<SiteModelFeaturePolicySnapshot> LoadSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var row = await _context.ModelSettings
                .AsNoTracking()
                .Select(s => new
                {
                    s.EnableModelComments,
                    s.CommentsDisabledMode,
                    s.EnableModelCollections,
                    s.CollectionsDisabledMode,
                    s.LimitTotalModels,
                    s.MaxModelsPerUser
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
            {
                return SiteModelFeaturePolicySnapshot.FailClosed;
            }

            return new SiteModelFeaturePolicySnapshot(
                row.EnableModelComments,
                ResolveAccess(row.EnableModelComments, row.CommentsDisabledMode),
                row.EnableModelCollections,
                ResolveAccess(row.EnableModelCollections, row.CollectionsDisabledMode),
                row.LimitTotalModels,
                row.MaxModelsPerUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read site model feature policy; failing closed");
            return SiteModelFeaturePolicySnapshot.FailClosed;
        }
    }

    private static FeatureAccessLevel ResolveAccess(bool enabled, FeatureDisabledMode disabledMode) =>
        enabled
            ? FeatureAccessLevel.Full
            : disabledMode == FeatureDisabledMode.Hidden
                ? FeatureAccessLevel.Hidden
                : FeatureAccessLevel.ReadOnly;
}