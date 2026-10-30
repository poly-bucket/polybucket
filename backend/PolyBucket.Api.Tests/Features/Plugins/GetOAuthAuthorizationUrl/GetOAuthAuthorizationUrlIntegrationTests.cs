using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetOAuthAuthorizationUrl;

[Collection("TestCollection")]
public class GetOAuthAuthorizationUrlIntegrationTests : BaseIntegrationTest
{
    public GetOAuthAuthorizationUrlIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetOAuthAuthorizationUrl returns Unauthorized.")]
    public async Task GetOAuthAuthorizationUrl_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/plugins/oauth/authorize/test?redirectUri=https://example.com/cb");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
