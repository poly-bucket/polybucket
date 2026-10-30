using System.Collections.Generic;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.HandleOAuthCallback;

[Collection("TestCollection")]
public class HandleOAuthCallbackIntegrationTests : BaseIntegrationTest
{
    public HandleOAuthCallbackIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, HandleOAuthCallback returns Unauthorized.")]
    public async Task HandleOAuthCallback_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/plugins/oauth/callback/test", new { code = "c", redirectUri = "https://example.com/cb" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
