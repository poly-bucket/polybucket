using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Plugins.Http;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Database.Persistence;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.Http;

public class MarketplaceSettingsControllerTests
{
    [Fact(DisplayName = "When marketplace base URL is saved, GetMarketplaceBaseUrl returns the normalized value.")]
    public async Task UpdateAndGetMarketplaceBaseUrl_PersistsValue()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new PolyBucketDbContext(options);
        var controller = new MarketplaceSettingsController(context);

        // Act
        var update = await controller.UpdateMarketplaceBaseUrl(
            new UpdateMarketplaceBaseUrlRequest { BaseUrl = "https://marketplace.example.com/" },
            CancellationToken.None);
        var get = await controller.GetMarketplaceBaseUrl(CancellationToken.None);

        // Assert
        update.Result.ShouldBeOfType<OkObjectResult>();
        get.Result.ShouldBeOfType<OkObjectResult>();
        var body = ((OkObjectResult)get.Result!).Value.ShouldBeOfType<MarketplaceBaseUrlResponse>();
        body.BaseUrl.ShouldBe("https://marketplace.example.com");
    }
}
