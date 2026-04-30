using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Models.GetModelById.Domain;
using PolyBucket.Api.Features.Models.GetModelById.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GetModelById;

public class GetModelByIdControllerTests
{
    private readonly Mock<IMediator> _mediator;
    private readonly Mock<ILogger<GetModelByIdController>> _logger;
    private readonly GetModelByIdController _controller;

    public GetModelByIdControllerTests()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<GetModelByIdController>>();
        _controller = new GetModelByIdController(_mediator.Object, _logger.Object);
    }

    [Fact(DisplayName = "When getting a model by id and the model exists, the get model by id controller returns Ok with the model.")]
    public async Task GetModel_ReturnsOk_WhenModelFound()
    {
        var id = Guid.NewGuid();
        var model = new PolyBucket.Api.Common.Models.Model { Id = id, Name = "Test", Description = "Desc" };
        var expected = new GetModelByIdResponse { Model = model };
        _mediator.Setup(m => m.Send(It.Is<GetModelByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _controller.GetModel(id);
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(expected);
    }

    [Fact(DisplayName = "When getting a model by id and the model is missing, the get model by id controller returns NotFound.")]
    public async Task GetModel_ReturnsNotFound_WhenModelMissing()
    {
        var id = Guid.NewGuid();
        _mediator.Setup(m => m.Send(It.IsAny<GetModelByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());
        var result = await _controller.GetModel(id);
        result.Result.ShouldBeOfType<NotFoundResult>();
    }
} 