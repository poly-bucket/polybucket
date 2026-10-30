using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.DeleteAllCommentsForTarget;

[Collection("TestCollection")]
public class DeleteAllCommentsForTargetIntegrationTests : BaseIntegrationTest
{
    public DeleteAllCommentsForTargetIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, DeleteAllCommentsForTarget returns Unauthorized.")]
    public async Task DeleteAllCommentsForTarget_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/comments/target/model/{System.Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
