using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetExtensibleThemeConfiguration;

[Collection("TestCollection")]
public class GetExtensibleThemeConfigurationIntegrationTests : BaseIntegrationTest
{
    public GetExtensibleThemeConfigurationIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, GetExtensibleThemeConfiguration returns the expected status.")]
    public async Task GetExtensibleThemeConfiguration_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("GET"), "/api/system-settings/extensible-theme/configuration");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
