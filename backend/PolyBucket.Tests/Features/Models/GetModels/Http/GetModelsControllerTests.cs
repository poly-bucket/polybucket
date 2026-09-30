using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Models.GetModels.Domain;
using PolyBucket.Api.Features.Models.GetModels.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GetModels.Http;

public class GetModelsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<ILogger<GetModelsController>> _logger = new();

    [Fact(DisplayName = "When listing models, the controller returns Ok with the mediator response.")]
    public async Task GetModels_ReturnsOk()
    {
        // Arrange
        var query = new GetModelsQuery { Page = 1, Take = 10 };
        var response = new GetModelsResponse { Models = new List<PolyBucket.Api.Features.Models.Common.ModelDto>(), TotalCount = 0, Page = 1, TotalPages = 0 };
        _mediator.SetupSend<GetModelsQuery, GetModelsResponse>(response);
        var controller = new GetModelsController(_mediator.Object, _logger.Object);

        // Act
        var result = await controller.GetModels(query);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        _mediator.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }
}
