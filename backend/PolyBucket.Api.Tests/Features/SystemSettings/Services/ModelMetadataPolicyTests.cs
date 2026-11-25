using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Services;

public class ModelMetadataPolicyTests : IDisposable
{
    private readonly PolyBucketDbContext _dbContext;
    private readonly ModelMetadataPolicy _policy;

    public ModelMetadataPolicyTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PolyBucketDbContext(options);
        _policy = new ModelMetadataPolicy(
            _dbContext,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<ModelMetadataPolicy>.Instance);
    }

    [Fact(DisplayName = "When ModelSettings are stored, the metadata policy snapshot matches the row.")]
    public async Task GetSnapshot_MapsModelSettings()
    {
        // Arrange
        _dbContext.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            RequireModelDescription = true,
            MinDescriptionLength = 5,
            MaxDescriptionLength = 500,
            AllowNSFWContent = false,
            RequireThumbnail = true,
            MaxTagsPerModel = 7,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _policy.Invalidate();

        // Act
        var snapshot = await _policy.GetSnapshotAsync();

        // Assert
        snapshot.RequireModelDescription.ShouldBeTrue();
        snapshot.MinDescriptionLength.ShouldBe(5);
        snapshot.MaxDescriptionLength.ShouldBe(500);
        snapshot.AllowNsfwContent.ShouldBeFalse();
        snapshot.RequireThumbnail.ShouldBeTrue();
        snapshot.MaxTagsPerModel.ShouldBe(7);
    }

    [Fact(DisplayName = "When remix flags are stored, the metadata policy snapshot includes them.")]
    public async Task GetSnapshot_IncludesRemixFlags()
    {
        // Arrange
        _dbContext.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            AllowModelRemixing = false,
            RequireRemixAttribution = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _policy.Invalidate();

        // Act
        var snapshot = await _policy.GetSnapshotAsync();

        // Assert
        snapshot.AllowModelRemixing.ShouldBeFalse();
        snapshot.RequireRemixAttribution.ShouldBeFalse();
    }

    public void Dispose() => _dbContext.Dispose();
}