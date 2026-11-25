using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Services;

public class SiteModelDownloadPolicyTests : IDisposable
{
    private readonly PolyBucketDbContext _dbContext;
    private readonly SiteModelDownloadPolicy _policy;

    public SiteModelDownloadPolicyTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PolyBucketDbContext(options);
        _policy = new SiteModelDownloadPolicy(_dbContext, new MemoryCache(new MemoryCacheOptions()), NullLogger<SiteModelDownloadPolicy>.Instance);
    }

    [Theory(DisplayName = "Download policy reflects AllowAnonDownloads and EnableModelDownloads.")]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task Snapshot_MatchesModelSettings(bool allowAnon, bool enableDownloads)
    {
        // Arrange
        _dbContext.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            AllowAnonDownloads = allowAnon,
            EnableModelDownloads = enableDownloads,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var snapshot = await _policy.GetSnapshotAsync();

        // Assert
        snapshot.AllowAnonDownloads.ShouldBe(allowAnon);
        snapshot.EnableModelDownloads.ShouldBe(enableDownloads);
        (await _policy.IsAnonymousModelDownloadAllowedAsync()).ShouldBe(allowAnon);
        (await _policy.AreModelDownloadsEnabledAsync()).ShouldBe(enableDownloads);
    }

    [Fact(DisplayName = "When model settings are missing, download policy fails closed.")]
    public async Task MissingSettings_FailClosed()
    {
        // Act
        var snapshot = await _policy.GetSnapshotAsync();

        // Assert
        snapshot.AllowAnonDownloads.ShouldBeFalse();
        snapshot.EnableModelDownloads.ShouldBeFalse();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}