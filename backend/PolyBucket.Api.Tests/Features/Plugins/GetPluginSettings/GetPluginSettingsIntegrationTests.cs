using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetPluginSettings;

[Collection("TestCollection")]
public class GetPluginSettingsIntegrationTests : BaseIntegrationTest
{
    public GetPluginSettingsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetPluginSettings returns Unauthorized.")]
    public async Task GetPluginSettings_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/plugins/test/settings");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
