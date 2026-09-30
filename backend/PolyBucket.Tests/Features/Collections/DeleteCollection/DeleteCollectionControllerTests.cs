using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Collections.DeleteCollection.Domain;
using PolyBucket.Api.Features.Collections.DeleteCollection.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections.DeleteCollection;

public class DeleteCollectionControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly DeleteCollectionController _controller;

    public DeleteCollectionControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When deletion succeeds, DeleteCollection returns NoContent.")]
    public async Task DeleteCollection_Success_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mediator
            .Setup(m => m.Send(It.IsAny<DeleteCollectionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteCollection(id);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
        _mediator.Verify(
            m => m.Send(It.Is<DeleteCollectionCommand>(c => c.Id == id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "When the mediator throws, DeleteCollection propagates the exception.")]
    public async Task DeleteCollection_MediatorThrows_Propagates()
    {
        // Arrange
        _mediator
            .Setup(m => m.Send(It.IsAny<DeleteCollectionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("fail"));

        // Act
        var act = () => _controller.DeleteCollection(Guid.NewGuid());

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
    }
}
