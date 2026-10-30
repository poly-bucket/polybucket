using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateThemeSettings;

[Collection("TestCollection")]
public class UpdateThemeSettingsIntegrationTests : BaseIntegrationTest
{
    public UpdateThemeSettingsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, UpdateThemeSettings returns the expected status.")]
    public async Task UpdateThemeSettings_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("PUT"), "/api/system-settings/theme");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
