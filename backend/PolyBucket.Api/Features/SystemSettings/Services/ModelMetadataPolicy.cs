using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public sealed class ModelMetadataPolicy(
    PolyBucketDbContext context,
    IMemoryCache cache,
    ILogger<ModelMetadataPolicy> logger) : IModelMetadataPolicy
{
    internal const string CacheKey = "model-metadata-policy";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly PolyBucketDbContext _context = context;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<ModelMetadataPolicy> _logger = logger;

    public async Task<ModelMetadataPolicySnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out ModelMetadataPolicySnapshot? cached) && cached != null)
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

    private async Task<ModelMetadataPolicySnapshot> LoadSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _context.ModelSettings
                .AsNoTracking()
                .Select(s => new
                {
                    s.RequireModelDescription,
                    s.MinDescriptionLength,
                    s.MaxDescriptionLength,
                    s.RequireLicenseSelection,
                    s.AllowCustomLicenses,
                    s.AllowNSFWContent,
                    s.AllowAIGeneratedContent,
                    s.RequireThumbnail,
                    s.MaxTagsPerModel,
                    s.RequireModelTags,
                    s.MaxCategoriesPerModel,
                    s.RequireCategorySelection,
                    s.AllowModelRemixing,
                    s.RequireRemixAttribution
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (settings == null)
            {
                return ModelMetadataPolicySnapshot.FailClosed;
            }

            return new ModelMetadataPolicySnapshot(
                settings.RequireModelDescription,
                settings.MinDescriptionLength,
                settings.MaxDescriptionLength,
                settings.RequireLicenseSelection,
                settings.AllowCustomLicenses,
                settings.AllowNSFWContent,
                settings.AllowAIGeneratedContent,
                settings.RequireThumbnail,
                settings.MaxTagsPerModel,
                settings.RequireModelTags,
                settings.MaxCategoriesPerModel,
                settings.RequireCategorySelection,
                settings.AllowModelRemixing,
                settings.RequireRemixAttribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read model metadata policy; using fail-closed defaults");
            return ModelMetadataPolicySnapshot.FailClosed;
        }
    }
}