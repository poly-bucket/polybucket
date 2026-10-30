using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.ReloadPlugins;

[Collection("TestCollection")]
public class ReloadPluginsIntegrationTests : BaseIntegrationTest
{
    public ReloadPluginsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, ReloadPlugins returns Unauthorized.")]
    public async Task ReloadPlugins_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/plugins/reload", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
