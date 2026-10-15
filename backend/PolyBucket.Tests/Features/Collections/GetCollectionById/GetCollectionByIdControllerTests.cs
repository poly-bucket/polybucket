using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Collections.GetCollectionById.Domain;
using PolyBucket.Api.Features.Collections.GetCollectionById.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections.GetCollectionById;

public class GetCollectionByIdControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly GetCollectionByIdController _controller;

    public GetCollectionByIdControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When the collection exists, GetCollectionById returns Ok.")]
    public async Task GetCollectionById_Found_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new GetCollectionByIdResponseDto { Id = id, Name = "Mine" };
        _mediator.SetupSend<GetCollectionByIdQuery, GetCollectionByIdResponseDto?>(dto);

        // Act
        var result = await _controller.GetCollectionById(id);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(dto);
    }

    [Fact(DisplayName = "When the collection is missing, GetCollectionById returns NotFound.")]
    public async Task GetCollectionById_Missing_ReturnsNotFound()
    {
        // Arrange
        _mediator
            .Setup(m => m.Send(It.IsAny<GetCollectionByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCollectionByIdResponseDto?)null);

        // Act
        var result = await _controller.GetCollectionById(Guid.NewGuid());

        // Assert
        result.Result.ShouldBeOfType<NotFoundResult>();
    }
}
