using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.RemoveCommentLike;

[Collection("TestCollection")]
public class RemoveCommentLikeIntegrationTests : BaseIntegrationTest
{
    public RemoveCommentLikeIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, RemoveCommentLike returns Unauthorized.")]
    public async Task RemoveCommentLike_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/comments/{System.Guid.NewGuid()}/like");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
