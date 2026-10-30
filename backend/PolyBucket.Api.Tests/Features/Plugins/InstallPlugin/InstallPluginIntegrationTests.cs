using System.Collections.Generic;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.InstallPlugin;

[Collection("TestCollection")]
public class InstallPluginIntegrationTests : BaseIntegrationTest
{
    public InstallPluginIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, InstallPlugin returns Unauthorized.")]
    public async Task InstallPlugin_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/plugins/install", new { source = "url", url = "https://example.com" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
