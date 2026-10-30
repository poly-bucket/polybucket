using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.ApplyStorageSetup;

[Collection("TestCollection")]
public class ApplyStorageSetupIntegrationTests : BaseIntegrationTest
{
    public ApplyStorageSetupIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, ApplyStorageSetup returns the expected status.")]
    public async Task ApplyStorageSetup_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/SystemSetup/apply-storage");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
