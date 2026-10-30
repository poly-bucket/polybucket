using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.SystemSettings.GetStorageProviders.Domain;
using PolyBucket.Api.Features.SystemSettings.GetStorageProviders.Http;
using PolyBucket.Storage;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.GetStorageProviders;

public class GetStorageProvidersControllerTests
{
    [Fact(DisplayName = "GetStorageProviders returns Ok with providers.")]
    public void GetStorageProviders_ReturnsOk()
    {
        // Arrange
        var service = new Mock<IGetStorageProvidersService>();
        service.Setup(s => s.GetStorageProviders()).Returns(new StorageProvidersResponseDto());
        var controller = new GetStorageProvidersController(service.Object);

        // Act
        var result = controller.GetStorageProviders();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
    }
}
