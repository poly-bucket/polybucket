using System.Collections.Generic;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.UpdatePluginSettings;

[Collection("TestCollection")]
public class UpdatePluginSettingsIntegrationTests : BaseIntegrationTest
{
    public UpdatePluginSettingsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, UpdatePluginSettings returns Unauthorized.")]
    public async Task UpdatePluginSettings_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("/api/plugins/test/settings", new { settings = new Dictionary<string, object>() });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
