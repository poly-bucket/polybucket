using System.Collections.Generic;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.UpdateThemeSettings;

[Collection("TestCollection")]
public class UpdateThemeSettingsIntegrationTests : BaseIntegrationTest
{
    public UpdateThemeSettingsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, UpdateThemeSettings returns Unauthorized.")]
    public async Task UpdateThemeSettings_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("/api/plugins/themes/test/settings", new { primaryColor = "#000" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
