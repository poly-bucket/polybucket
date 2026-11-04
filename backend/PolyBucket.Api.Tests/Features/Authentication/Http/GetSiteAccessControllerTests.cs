using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;
using PolyBucket.Api.Features.Authentication.GetSiteAccess.Http;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.Http;

public class GetSiteAccessControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _dbContext;
    private readonly GetSiteAccessController _controller;

    public GetSiteAccessControllerTests()
    {
        var dbOptions = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PolyBucketDbContext(dbOptions);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var privacy = new SitePrivacyState(_dbContext, cache, NullLogger<SitePrivacyState>.Instance);
        _controller = new GetSiteAccessController(new GetSiteAccessService(privacy));
    }

    [Fact(DisplayName = "When no site setup exists, site access reports public browsing is allowed.")]
    public async Task GetSiteAccess_NoSetup_ReturnsPublicBrowsingAllowed()
    {
        // Act
        var result = await _controller.GetSiteAccess(CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldNotBeNull();
        var allowed = ok.Value!.GetType().GetProperty("allowPublicBrowsing")!.GetValue(ok.Value);
        allowed.ShouldBe(true);
    }

    [Fact(DisplayName = "When public browsing is disabled in site setup, site access reports private site.")]
    public async Task GetSiteAccess_PrivateSite_ReturnsFalse()
    {
        // Arrange
        _dbContext.SystemSetups.Add(new SystemSetup
        {
            Id = Guid.NewGuid(),
            AllowPublicBrowsing = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetSiteAccess(CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        var allowed = ok.Value!.GetType().GetProperty("allowPublicBrowsing")!.GetValue(ok.Value);
        allowed.ShouldBe(false);
    }

    [Fact(DisplayName = "When public browsing is enabled in site setup, site access reports open browsing.")]
    public async Task GetSiteAccess_PublicSite_ReturnsTrue()
    {
        // Arrange
        _dbContext.SystemSetups.Add(new SystemSetup
        {
            Id = Guid.NewGuid(),
            AllowPublicBrowsing = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetSiteAccess(CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        var allowed = ok.Value!.GetType().GetProperty("allowPublicBrowsing")!.GetValue(ok.Value);
        allowed.ShouldBe(true);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
