using System.Collections.Generic;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.TransformMetadata;

[Collection("TestCollection")]
public class TransformMetadataIntegrationTests : BaseIntegrationTest
{
    public TransformMetadataIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, TransformMetadata returns Unauthorized.")]
    public async Task TransformMetadata_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/plugins/metadata/transform/model", new { fieldValues = new Dictionary<string, object>() });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
