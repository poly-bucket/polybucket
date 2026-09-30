using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.Models.GetModelPreview.Domain;
using PolyBucket.Api.Features.Models.GetModelPreview.Http;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GetModelPreview.Http;

public class GetModelPreviewControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "When a preview exists, the controller returns Ok with preview metadata.")]
    public async Task GetModelPreview_ReturnsOk()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var response = new GetModelPreviewResponse
        {
            ModelId = modelId,
            Size = "thumbnail",
            Status = PreviewStatus.Completed,
            PreviewUrl = "/previews/x.png"
        };
        _mediator.SetupSend<GetModelPreviewQuery, GetModelPreviewResponse>(response);
        var controller = new GetModelPreviewController(_mediator.Object);

        // Act
        var result = await controller.GetModelPreview(modelId, "thumbnail");

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        _mediator.Verify(
            m => m.Send(It.Is<GetModelPreviewQuery>(q => q.ModelId == modelId && q.Size == "thumbnail"), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
