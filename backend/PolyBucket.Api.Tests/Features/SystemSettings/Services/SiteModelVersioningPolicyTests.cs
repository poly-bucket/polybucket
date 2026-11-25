using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.ACL;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Services;

public class SiteModelVersioningPolicyTests : IDisposable
{
    private readonly PolyBucketDbContext _dbContext;
    private readonly Mock<IPermissionService> _permissions = new();
    private readonly SiteModelVersioningPolicy _policy;

    public SiteModelVersioningPolicyTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PolyBucketDbContext(options);
        _policy = new SiteModelVersioningPolicy(
            _dbContext,
            new MemoryCache(new MemoryCacheOptions()),
            _permissions.Object,
            NullLogger<SiteModelVersioningPolicy>.Instance);
    }

    [Theory(DisplayName = "Versioning policy reflects EnableModelVersioning.")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Snapshot_MatchesModelSettings(bool enabled)
    {
        // Arrange
        _dbContext.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            EnableModelVersioning = enabled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var snapshot = await _policy.GetSnapshotAsync();

        // Assert
        snapshot.EnableModelVersioning.ShouldBe(enabled);
        (await _policy.IsModelVersioningEnabledAsync()).ShouldBe(enabled);
    }

    [Fact(DisplayName = "When model settings are missing, versioning policy fails closed.")]
    public async Task MissingSettings_FailClosed()
    {
        // Act
        var snapshot = await _policy.GetSnapshotAsync();

        // Assert
        snapshot.EnableModelVersioning.ShouldBeFalse();
        (await _policy.IsModelVersioningEnabledAsync()).ShouldBeFalse();
    }

    [Fact(DisplayName = "When versioning is disabled, the model owner can view version history.")]
    public async Task DisabledMode_OwnerCanViewHistory()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, authorId.ToString())],
            "Test"));

        // Act
        var allowed = await _policy.CanViewVersionHistoryWhenDisabledAsync(user, authorId, CancellationToken.None);

        // Assert
        allowed.ShouldBeTrue();
    }

    [Fact(DisplayName = "When versioning is disabled, strangers cannot view version history.")]
    public async Task DisabledMode_StrangerCannotViewHistory()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
            "Test"));
        _permissions.Setup(p => p.IsAdminAsync(It.IsAny<Guid>())).ReturnsAsync(false);
        _permissions.Setup(p => p.GetUserRoleAsync(It.IsAny<Guid>())).ReturnsAsync((Role?)null);

        // Act
        var allowed = await _policy.CanViewVersionHistoryWhenDisabledAsync(user, Guid.NewGuid(), CancellationToken.None);

        // Assert
        allowed.ShouldBeFalse();
    }

    [Fact(DisplayName = "When versioning is disabled, admins can view version history.")]
    public async Task DisabledMode_AdminCanViewHistory()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, adminId.ToString())],
            "Test"));
        _permissions.Setup(p => p.IsAdminAsync(adminId)).ReturnsAsync(true);

        // Act
        var allowed = await _policy.CanViewVersionHistoryWhenDisabledAsync(user, Guid.NewGuid(), CancellationToken.None);

        // Assert
        allowed.ShouldBeTrue();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}