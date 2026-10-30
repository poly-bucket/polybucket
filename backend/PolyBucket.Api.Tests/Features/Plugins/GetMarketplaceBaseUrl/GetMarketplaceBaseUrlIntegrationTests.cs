using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetMarketplaceBaseUrl;

[Collection("TestCollection")]
public class GetMarketplaceBaseUrlIntegrationTests : BaseIntegrationTest
{
    public GetMarketplaceBaseUrlIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetMarketplaceBaseUrl returns Unauthorized.")]
    public async Task GetMarketplaceBaseUrl_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/admin/marketplace/base-url");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
