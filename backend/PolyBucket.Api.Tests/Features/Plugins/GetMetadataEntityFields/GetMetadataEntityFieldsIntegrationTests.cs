using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetMetadataEntityFields;

[Collection("TestCollection")]
public class GetMetadataEntityFieldsIntegrationTests : BaseIntegrationTest
{
    public GetMetadataEntityFieldsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetMetadataEntityFields returns Unauthorized.")]
    public async Task GetMetadataEntityFields_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/plugins/metadata/fields/model");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
