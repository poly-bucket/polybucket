using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateFontAwesomeSettings;

[Collection("TestCollection")]
public class UpdateFontAwesomeSettingsIntegrationTests : BaseIntegrationTest
{
    public UpdateFontAwesomeSettingsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, UpdateFontAwesomeSettings returns the expected status.")]
    public async Task UpdateFontAwesomeSettings_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/admin/fontawesome-settings");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
