using System.Net;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.TestStorageConfiguration;

[Collection("TestCollection")]
public class TestStorageConfigurationIntegrationTests : BaseIntegrationTest
{
    public TestStorageConfigurationIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When unauthenticated, TestStorageConfiguration returns the expected status.")]
    public async Task TestStorageConfiguration_Unauthenticated_ReturnsExpectedStatus()
    {
        // Arrange
        await ResetStateAsync();
        var client = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/system-settings/storage/test");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
