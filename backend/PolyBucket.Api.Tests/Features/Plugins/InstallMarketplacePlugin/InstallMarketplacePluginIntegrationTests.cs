using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.InstallMarketplacePlugin;

[Collection("TestCollection")]
public class InstallMarketplacePluginIntegrationTests : BaseIntegrationTest
{
    public InstallMarketplacePluginIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, InstallMarketplacePlugin returns Unauthorized.")]
    public async Task InstallMarketplacePlugin_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/plugins/marketplace/test/install", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
