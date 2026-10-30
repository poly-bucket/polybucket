using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.SetActiveExtensibleTheme;

[Collection("TestCollection")]
public class SetActiveExtensibleThemeIntegrationTests : BaseIntegrationTest
{
    public SetActiveExtensibleThemeIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, SetActiveExtensibleTheme returns the expected status.")]
    public async Task SetActiveExtensibleTheme_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/system-settings/extensible-theme/active/test-theme");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
