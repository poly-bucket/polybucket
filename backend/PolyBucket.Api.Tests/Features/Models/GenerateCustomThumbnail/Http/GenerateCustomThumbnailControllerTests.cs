using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Domain;
using PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Http;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Models.GenerateCustomThumbnail.Http;

public class GenerateCustomThumbnailControllerTests
{
    private readonly Mock<IGenerateCustomThumbnailService> _service = new();

    [Fact(DisplayName = "When settings are missing, the controller returns BadRequest.")]
    public async Task GenerateCustomThumbnail_NoSettings_ReturnsBadRequest()
    {
        // Arrange
        var controller = new GenerateCustomThumbnailController(_service.Object);
        var request = new GenerateCustomThumbnailRequest { Settings = null };

        // Act
        var result = await controller.GenerateCustomThumbnail(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When settings are provided, the controller returns Ok with the service response.")]
    public async Task GenerateCustomThumbnail_Valid_ReturnsOk()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var response = new GenerateCustomThumbnailResponse { ModelId = modelId, Size = "thumbnail", IsQueued = true };
        _service.Setup(s => s.GenerateAsync(It.IsAny<GenerateCustomThumbnailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = new GenerateCustomThumbnailController(_service.Object);
        var request = new GenerateCustomThumbnailRequest
        {
            ModelFileUrl = "https://example.com/model.stl",
            FileType = "stl",
            Settings = new PreviewGenerationSettings()
        };

        // Act
        var result = await controller.GenerateCustomThumbnail(modelId, request, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
