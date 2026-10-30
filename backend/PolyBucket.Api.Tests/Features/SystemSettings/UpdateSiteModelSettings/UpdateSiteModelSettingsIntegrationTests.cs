using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateSiteModelSettings;

[Collection("TestCollection")]
public class UpdateSiteModelSettingsIntegrationTests : BaseIntegrationTest
{
    public UpdateSiteModelSettingsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, UpdateSiteModelSettings returns the expected status.")]
    public async Task UpdateSiteModelSettings_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("PUT"), "/api/system-settings/model-settings");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
