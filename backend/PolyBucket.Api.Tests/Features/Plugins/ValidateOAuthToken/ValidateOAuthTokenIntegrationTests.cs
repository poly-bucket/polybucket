using System.Collections.Generic;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.ValidateOAuthToken;

[Collection("TestCollection")]
public class ValidateOAuthTokenIntegrationTests : BaseIntegrationTest
{
    public ValidateOAuthTokenIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, ValidateOAuthToken returns Unauthorized.")]
    public async Task ValidateOAuthToken_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/plugins/oauth/validate/test", new { accessToken = "t" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
