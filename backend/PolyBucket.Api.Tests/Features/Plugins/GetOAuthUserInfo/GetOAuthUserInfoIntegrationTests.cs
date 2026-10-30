using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.GetOAuthUserInfo;

[Collection("TestCollection")]
public class GetOAuthUserInfoIntegrationTests : BaseIntegrationTest
{
    public GetOAuthUserInfoIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetOAuthUserInfo returns Unauthorized.")]
    public async Task GetOAuthUserInfo_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/plugins/oauth/userinfo/test?accessToken=t");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
