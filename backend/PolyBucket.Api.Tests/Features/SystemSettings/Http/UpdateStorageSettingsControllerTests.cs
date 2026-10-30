using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Http;
using PolyBucket.Api.Features.SystemSettings.UpdateStorageSettings.Http;
using PolyBucket.Storage;
using PolyBucket.Storage.UpdateStorageSettings;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.Http;

public class UpdateStorageSettingsControllerTests
{
    [Fact(DisplayName = "When storage settings are invalid, update returns BadRequest.")]
    public async Task UpdateStorageSettings_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var service = new Mock<IUpdateStorageSettingsService>();
        service
            .Setup(s => s.UpdateAsync(It.IsAny<StorageSettingsUpdate>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StorageSettingsValidationException(["Storage provider 'aws' is not supported. Supported providers: minio, silo, seaweedfs."]));

        var controller = new UpdateStorageSettingsController(service.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var request = new UpdateStorageSettingsRequest
        {
            Provider = "aws",
            Endpoint = "localhost",
            Port = 9000,
            BucketName = "b",
            AccessKey = "a",
            SecretKey = "s"
        };

        // Act
        var result = await controller.UpdateStorageSettings(request);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
}
