using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Http;
using PolyBucket.Api.Tests.Testing;
using PolyBucket.Storage;
using PolyBucket.Storage.GetStorageSettings;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Http;

public class GetStorageSettingsControllerTests
{
    private readonly Mock<IGetStorageSettingsService> _service = new();

    [Fact(DisplayName = "When settings exist, GetStorageSettings returns Ok.")]
    public async Task GetStorageSettings_ReturnsOk()
    {
        // Arrange
        var dto = new StorageSettingsDto { Provider = "minio", IsConfigured = false };
        _service.Setup(s => s.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var controller = new GetStorageSettingsController(_service.Object);

        // Act
        var result = await controller.GetStorageSettings();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(dto);
    }
}
