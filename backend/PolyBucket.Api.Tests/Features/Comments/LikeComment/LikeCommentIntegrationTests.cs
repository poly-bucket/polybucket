using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.LikeComment;

[Collection("TestCollection")]
public class LikeCommentIntegrationTests : BaseIntegrationTest
{
    public LikeCommentIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, LikeComment returns Unauthorized.")]
    public async Task LikeComment_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync($"/api/comments/{System.Guid.NewGuid()}/like", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
