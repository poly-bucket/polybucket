using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetMetadataSchema;

[Collection("TestCollection")]
public class GetMetadataSchemaIntegrationTests : BaseIntegrationTest
{
    public GetMetadataSchemaIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetMetadataSchema returns Unauthorized.")]
    public async Task GetMetadataSchema_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/plugins/metadata/schema/model");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
