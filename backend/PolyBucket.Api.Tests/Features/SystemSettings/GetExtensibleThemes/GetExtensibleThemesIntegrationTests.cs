using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetExtensibleThemes;

[Collection("TestCollection")]
public class GetExtensibleThemesIntegrationTests : BaseIntegrationTest
{
    public GetExtensibleThemesIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, GetExtensibleThemes returns the expected status.")]
    public async Task GetExtensibleThemes_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("GET"), "/api/system-settings/extensible-theme/themes");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
