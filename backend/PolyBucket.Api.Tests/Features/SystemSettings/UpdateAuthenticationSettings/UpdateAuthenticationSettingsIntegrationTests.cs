using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.UpdateAuthenticationSettings;

[Collection("TestCollection")]
public class UpdateAuthenticationSettingsIntegrationTests : BaseIntegrationTest
{
    public UpdateAuthenticationSettingsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, UpdateAuthenticationSettings returns the expected status.")]
    public async Task UpdateAuthenticationSettings_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("PUT"), "/api/system-settings/auth");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
