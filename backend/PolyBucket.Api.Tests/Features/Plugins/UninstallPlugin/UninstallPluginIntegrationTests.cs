using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.UninstallPlugin;

[Collection("TestCollection")]
public class UninstallPluginIntegrationTests : BaseIntegrationTest
{
    public UninstallPluginIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, UninstallPlugin returns Unauthorized.")]
    public async Task UninstallPlugin_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/plugins/test-plugin");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
