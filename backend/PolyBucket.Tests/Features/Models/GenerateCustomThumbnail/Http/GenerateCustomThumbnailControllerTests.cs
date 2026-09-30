using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Domain;
using PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Http;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Services;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GenerateCustomThumbnail.Http;

public class GenerateCustomThumbnailControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When settings are missing, the controller returns BadRequest.")]
    public async Task GenerateCustomThumbnail_NoSettings_ReturnsBadRequest()
    {
        // Arrange
        var controller = new GenerateCustomThumbnailController(_mediator.Object);
        var request = new GenerateCustomThumbnailRequest { Settings = null };

        // Act
        var result = await controller.GenerateCustomThumbnail(Guid.NewGuid(), request);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _mediator.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When settings are provided, the controller returns Ok with the handler response.")]
    public async Task GenerateCustomThumbnail_Valid_ReturnsOk()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var response = new GenerateCustomThumbnailResponse { ModelId = modelId, Size = "thumbnail", IsQueued = true };
        _mediator.SetupSend<GenerateCustomThumbnailCommand, GenerateCustomThumbnailResponse>(response);
        var controller = new GenerateCustomThumbnailController(_mediator.Object);
        var request = new GenerateCustomThumbnailRequest
        {
            ModelFileUrl = "https://example.com/model.stl",
            FileType = "stl",
            Settings = new PreviewGenerationSettings()
        };

        // Act
        var result = await controller.GenerateCustomThumbnail(modelId, request);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
