using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetMarketplaceCategories;

[Collection("TestCollection")]
public class GetMarketplaceCategoriesIntegrationTests : BaseIntegrationTest
{
    public GetMarketplaceCategoriesIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetMarketplaceCategories returns Unauthorized.")]
    public async Task GetMarketplaceCategories_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/plugins/marketplace/categories");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
