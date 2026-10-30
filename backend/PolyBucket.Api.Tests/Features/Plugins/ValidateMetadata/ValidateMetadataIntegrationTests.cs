using System.Collections.Generic;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.ValidateMetadata;

[Collection("TestCollection")]
public class ValidateMetadataIntegrationTests : BaseIntegrationTest
{
    public ValidateMetadataIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, ValidateMetadata returns Unauthorized.")]
    public async Task ValidateMetadata_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/plugins/metadata/validate/model/id1", new { fieldValues = new Dictionary<string, object>() });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
