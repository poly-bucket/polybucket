using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetInstalledPluginAsset;

[Collection("TestCollection")]
public class GetInstalledPluginAssetIntegrationTests : BaseIntegrationTest
{
    public GetInstalledPluginAssetIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetInstalledPluginAsset returns Unauthorized.")]
    public async Task GetInstalledPluginAsset_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/plugins/installed/test/assets/main.css");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
