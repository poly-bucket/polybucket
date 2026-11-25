using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Services;

public class SiteModelAdvancedPolicyTests : IDisposable
{
    private readonly PolyBucketDbContext _dbContext;
    private readonly SiteModelAdvancedPolicy _policy;

    public SiteModelAdvancedPolicyTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PolyBucketDbContext(options);
        _policy = new SiteModelAdvancedPolicy(
            _dbContext,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<SiteModelAdvancedPolicy>.Instance);
    }

    [Fact(DisplayName = "When ModelSettings exist, the advanced policy snapshot matches the row.")]
    public async Task GetSnapshot_MapsModelSettings()
    {
        // Arrange
        _dbContext.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            EnableModelImport = true,
            EnableModelAnalytics = false,
            EnableModelReporting = true,
            EnableModelSharing = false,
            EnableModelDeletion = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _policy.Invalidate();

        // Act
        var snapshot = await _policy.GetSnapshotAsync();

        // Assert
        snapshot.EnableModelImport.ShouldBeTrue();
        snapshot.EnableModelAnalytics.ShouldBeFalse();
        snapshot.EnableModelReporting.ShouldBeTrue();
        snapshot.EnableModelSharing.ShouldBeFalse();
        snapshot.EnableModelDeletion.ShouldBeTrue();
    }

    [Fact(DisplayName = "When ModelSettings row is missing, the advanced policy fails closed.")]
    public async Task GetSnapshot_NoRow_FailsClosed()
    {
        // Act
        var snapshot = await _policy.GetSnapshotAsync();

        // Assert
        snapshot.ShouldBe(SiteModelAdvancedPolicySnapshot.FailClosed);
    }

    public void Dispose() => _dbContext.Dispose();
}