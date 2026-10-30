using System.Collections.Generic;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.UpdatePluginStatus;

[Collection("TestCollection")]
public class UpdatePluginStatusIntegrationTests : BaseIntegrationTest
{
    public UpdatePluginStatusIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, UpdatePluginStatus returns Unauthorized.")]
    public async Task UpdatePluginStatus_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("/api/plugins/test/status", new { enabled = true });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
