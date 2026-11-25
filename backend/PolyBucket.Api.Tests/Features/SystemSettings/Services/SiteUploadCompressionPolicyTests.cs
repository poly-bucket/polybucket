using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Services;

public class SiteUploadCompressionPolicyTests
{
    [Fact]
    public async Task ShouldCompressAsync_WhenSiteFlagOff_ReturnsFalse()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new PolyBucketDbContext(options);
        context.SystemSetups.Add(new SystemSetup { EnableFileCompression = false });
        context.FileTypeSettings.Add(new FileTypeSettings
        {
            FileExtension = ".stl",
            Enabled = true,
            IsCompressible = true
        });
        await context.SaveChangesAsync();

        var policy = new SiteUploadCompressionPolicy(context);
        var result = await policy.ShouldCompressAsync("model.stl");

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldCompressAsync_WhenSiteFlagOnAndTypeCompressible_ReturnsTrue()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new PolyBucketDbContext(options);
        context.SystemSetups.Add(new SystemSetup { EnableFileCompression = true });
        context.FileTypeSettings.Add(new FileTypeSettings
        {
            FileExtension = ".stl",
            Enabled = true,
            IsCompressible = true
        });
        await context.SaveChangesAsync();

        var policy = new SiteUploadCompressionPolicy(context);
        var result = await policy.ShouldCompressAsync("model.stl");

        result.ShouldBeTrue();
    }
}