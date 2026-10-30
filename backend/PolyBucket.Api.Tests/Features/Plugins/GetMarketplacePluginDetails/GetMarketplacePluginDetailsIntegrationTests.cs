using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetMarketplacePluginDetails;

[Collection("TestCollection")]
public class GetMarketplacePluginDetailsIntegrationTests : BaseIntegrationTest
{
    public GetMarketplacePluginDetailsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetMarketplacePluginDetails returns Unauthorized.")]
    public async Task GetMarketplacePluginDetails_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/plugins/marketplace/test");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
