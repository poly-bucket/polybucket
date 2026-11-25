using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Database.Entities.ACL;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Services;

public class SiteModelFeaturePolicyTests : IDisposable
{
    private readonly PolyBucketDbContext _dbContext;
    private readonly SiteModelFeaturePolicy _policy;

    public SiteModelFeaturePolicyTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PolyBucketDbContext(options);
        var permissions = new Mock<IPermissionService>();
        permissions.Setup(p => p.IsAdminAsync(It.IsAny<Guid>())).ReturnsAsync(false);
        permissions.Setup(p => p.GetUserRoleAsync(It.IsAny<Guid>())).ReturnsAsync((Role?)null);
        _policy = new SiteModelFeaturePolicy(
            _dbContext,
            new MemoryCache(new MemoryCacheOptions()),
            permissions.Object,
            NullLogger<SiteModelFeaturePolicy>.Instance);
    }

    [Theory(DisplayName = "Comments access level reflects enable flag and disabled mode.")]
    [InlineData(true, FeatureDisabledMode.ReadOnly, FeatureAccessLevel.Full)]
    [InlineData(false, FeatureDisabledMode.ReadOnly, FeatureAccessLevel.ReadOnly)]
    [InlineData(false, FeatureDisabledMode.Hidden, FeatureAccessLevel.Hidden)]
    public async Task CommentsAccess_ReflectsSettings(bool enabled, FeatureDisabledMode mode, FeatureAccessLevel expected)
    {
        await SeedModelSettings(enabled, mode, true, FeatureDisabledMode.ReadOnly);

        var level = await _policy.GetCommentsAccessLevelAsync();
        level.ShouldBe(expected);
    }

    [Fact(DisplayName = "Quota check treats non-positive limits as unlimited.")]
    public async Task Quota_NonPositiveLimits_AllowCreation()
    {
        _dbContext.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            EnableModelComments = true,
            EnableModelCollections = true,
            LimitTotalModels = 0,
            MaxModelsPerUser = 0
        });
        await _dbContext.SaveChangesAsync();

        var outcome = await _policy.ValidateModelCreationQuotaAsync(Guid.NewGuid());
        outcome.Kind.ShouldBe(ModelCreationQuotaKind.Allowed);
    }

    private async Task SeedModelSettings(
        bool commentsEnabled,
        FeatureDisabledMode commentsMode,
        bool collectionsEnabled,
        FeatureDisabledMode collectionsMode)
    {
        _dbContext.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            EnableModelComments = commentsEnabled,
            CommentsDisabledMode = commentsMode,
            EnableModelCollections = collectionsEnabled,
            CollectionsDisabledMode = collectionsMode,
            LimitTotalModels = 1000,
            MaxModelsPerUser = 100
        });
        await _dbContext.SaveChangesAsync();
        _policy.Invalidate();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

}