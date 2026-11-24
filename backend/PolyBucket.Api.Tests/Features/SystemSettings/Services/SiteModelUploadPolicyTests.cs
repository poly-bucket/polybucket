using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.Models.Enums;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Services;

public class SiteModelUploadPolicyTests : IDisposable
{
    private readonly PolyBucketDbContext _dbContext;
    private readonly SiteModelUploadPolicy _policy;

    public SiteModelUploadPolicyTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PolyBucketDbContext(options);
        _policy = new SiteModelUploadPolicy(_dbContext, new MemoryCache(new MemoryCacheOptions()), NullLogger<SiteModelUploadPolicy>.Instance);
    }

    [Theory(DisplayName = "Upload policy truth table matches RequireLoginForUpload and AllowAnonUploads.")]
    [InlineData(true, false, false, true)]
    [InlineData(true, true, false, true)]
    [InlineData(false, false, false, true)]
    [InlineData(false, true, true, false)]
    public async Task Policy_TruthTable(
        bool requireLogin,
        bool allowAnon,
        bool expectAnonymousAllowed,
        bool expectLoginRequired)
    {
        // Arrange
        _dbContext.SystemSetups.Add(new SystemSetup
        {
            Id = Guid.NewGuid(),
            RequireLoginForUpload = requireLogin,
            DefaultModelPrivacy = PrivacySettings.Private,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        _dbContext.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            AllowAnonUploads = allowAnon,
            DefaultPrivacySetting = PrivacySettings.Public,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _policy.Invalidate();

        // Act
        var anon = await _policy.IsAnonymousModelUploadAllowedAsync();
        var login = await _policy.IsLoginRequiredForUnauthenticatedUploadAsync();

        // Assert
        anon.ShouldBe(expectAnonymousAllowed);
        login.ShouldBe(expectLoginRequired);
    }

    [Fact(DisplayName = "When SystemSetup is absent, default model privacy falls back to ModelSettings.")]
    public async Task DefaultPrivacy_UsesModelSettingsWhenNoSetup()
    {
        // Arrange
        _dbContext.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            AllowAnonUploads = false,
            DefaultPrivacySetting = PrivacySettings.Unlisted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _policy.Invalidate();

        // Act
        var privacy = await _policy.GetDefaultModelPrivacyAsync();

        // Assert
        privacy.ShouldBe(PrivacySettings.Unlisted);
    }

  [Fact(DisplayName = "When SystemSetup exists, default model privacy prefers site default over model configuration.")]
    public async Task DefaultPrivacy_PrefersSiteSetup()
    {
        // Arrange
        _dbContext.SystemSetups.Add(new SystemSetup
        {
            Id = Guid.NewGuid(),
            RequireLoginForUpload = true,
            DefaultModelPrivacy = PrivacySettings.Private,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        _dbContext.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            AllowAnonUploads = false,
            DefaultPrivacySetting = PrivacySettings.Public,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _policy.Invalidate();

        // Act
        var privacy = await _policy.GetDefaultModelPrivacyAsync();

        // Assert
        privacy.ShouldBe(PrivacySettings.Private);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}