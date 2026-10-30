using System.Collections.Generic;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.RefreshOAuthToken;

[Collection("TestCollection")]
public class RefreshOAuthTokenIntegrationTests : BaseIntegrationTest
{
    public RefreshOAuthTokenIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, RefreshOAuthToken returns Unauthorized.")]
    public async Task RefreshOAuthToken_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/plugins/oauth/refresh/test", new { refreshToken = "r" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
