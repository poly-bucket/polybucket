using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetThemeSettings;

[Collection("TestCollection")]
public class GetThemeSettingsIntegrationTests : BaseIntegrationTest
{
    public GetThemeSettingsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetThemeSettings returns Unauthorized.")]
    public async Task GetThemeSettings_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/plugins/themes/test/settings");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
