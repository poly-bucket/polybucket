using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Comments.GetCommentStatistics;

[Collection("TestCollection")]
public class GetCommentStatisticsIntegrationTests : BaseIntegrationTest
{
    public GetCommentStatisticsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When the request is unauthenticated, GetCommentStatistics returns Unauthorized.")]
    public async Task GetCommentStatistics_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/comments/statistics/model/{System.Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
