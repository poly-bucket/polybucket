using System.Collections.Generic;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.UpdateMarketplaceBaseUrl;

[Collection("TestCollection")]
public class UpdateMarketplaceBaseUrlIntegrationTests : BaseIntegrationTest
{
    public UpdateMarketplaceBaseUrlIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, UpdateMarketplaceBaseUrl returns Unauthorized.")]
    public async Task UpdateMarketplaceBaseUrl_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("/api/admin/marketplace/base-url", new { baseUrl = "https://example.com" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
