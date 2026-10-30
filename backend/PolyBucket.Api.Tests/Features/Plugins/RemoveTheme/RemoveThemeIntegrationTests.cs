using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.RemoveTheme;

[Collection("TestCollection")]
public class RemoveThemeIntegrationTests : BaseIntegrationTest
{
    public RemoveThemeIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, RemoveTheme returns Unauthorized.")]
    public async Task RemoveTheme_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/plugins/themes/test");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
