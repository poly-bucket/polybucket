using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.UnmoderateComment;

[Collection("TestCollection")]
public class UnmoderateCommentIntegrationTests : BaseIntegrationTest
{
    public UnmoderateCommentIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, UnmoderateComment returns Unauthorized.")]
    public async Task UnmoderateComment_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync($"/api/comments/{System.Guid.NewGuid()}/unmoderate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
