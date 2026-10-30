using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.DislikeComment;

[Collection("TestCollection")]
public class DislikeCommentIntegrationTests : BaseIntegrationTest
{
    public DislikeCommentIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, DislikeComment returns Unauthorized.")]
    public async Task DislikeComment_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync($"/api/comments/{System.Guid.NewGuid()}/dislike", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
