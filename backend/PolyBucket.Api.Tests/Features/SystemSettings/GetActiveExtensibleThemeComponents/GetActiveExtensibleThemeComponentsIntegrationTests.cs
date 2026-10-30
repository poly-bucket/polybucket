using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetActiveExtensibleThemeComponents;

[Collection("TestCollection")]
public class GetActiveExtensibleThemeComponentsIntegrationTests : BaseIntegrationTest
{
    public GetActiveExtensibleThemeComponentsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, GetActiveExtensibleThemeComponents returns the expected status.")]
    public async Task GetActiveExtensibleThemeComponents_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("GET"), "/api/system-settings/extensible-theme/components");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
