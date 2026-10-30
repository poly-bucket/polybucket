using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.GetCommentsForTarget;

[Collection("TestCollection")]
public class GetCommentsForTargetIntegrationTests : BaseIntegrationTest
{
    public GetCommentsForTargetIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetCommentsForTarget returns Unauthorized.")]
    public async Task GetCommentsForTarget_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/comments/target/model/{System.Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
