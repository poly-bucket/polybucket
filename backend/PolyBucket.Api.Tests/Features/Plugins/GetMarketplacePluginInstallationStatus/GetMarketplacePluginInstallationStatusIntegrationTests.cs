using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetMarketplacePluginInstallationStatus;

[Collection("TestCollection")]
public class GetMarketplacePluginInstallationStatusIntegrationTests : BaseIntegrationTest
{
    public GetMarketplacePluginInstallationStatusIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetMarketplacePluginInstallationStatus returns Unauthorized.")]
    public async Task GetMarketplacePluginInstallationStatus_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/plugins/marketplace/test/status");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
