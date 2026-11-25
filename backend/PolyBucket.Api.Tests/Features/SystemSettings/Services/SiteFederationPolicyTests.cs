using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Services;

public class SiteFederationPolicyTests : IDisposable
{
    private readonly PolyBucketDbContext _dbContext;
    private readonly SiteFederationPolicy _policy;

    public SiteFederationPolicyTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PolyBucketDbContext(options);
        _policy = new SiteFederationPolicy(
            _dbContext,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<SiteFederationPolicy>.Instance);
    }

    [Fact(DisplayName = "When SystemSetup exists, the federation policy snapshot matches the row.")]
    public async Task GetSnapshot_MapsSystemSetup()
    {
        // Arrange
        var setupId = Guid.NewGuid();
        _dbContext.SystemSetups.Add(new SystemSetup
        {
            Id = setupId,
            EnableFederation = true,
            InstanceName = "Peer One",
            InstanceDescription = "Test node",
            AdminContact = "admin@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _policy.Invalidate();

        // Act
        var snapshot = await _policy.GetSnapshotAsync();

        // Assert
        snapshot.SystemSetupId.ShouldBe(setupId);
        snapshot.IsEnabled.ShouldBeTrue();
        snapshot.InstanceName.ShouldBe("Peer One");
        snapshot.InstanceDescription.ShouldBe("Test node");
        snapshot.AdminContact.ShouldBe("admin@example.com");
        snapshot.HasInstanceName.ShouldBeTrue();
    }

    [Fact(DisplayName = "When SystemSetup row is missing, the federation policy is disabled.")]
    public async Task GetSnapshot_NoRow_Disabled()
    {
        // Act
        var snapshot = await _policy.GetSnapshotAsync();

        // Assert
        snapshot.ShouldBe(SiteFederationPolicySnapshot.Disabled);
    }

    public void Dispose() => _dbContext.Dispose();
}