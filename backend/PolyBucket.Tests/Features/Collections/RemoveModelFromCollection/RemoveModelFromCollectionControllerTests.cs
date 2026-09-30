using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Domain;
using PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections.RemoveModelFromCollection;

public class RemoveModelFromCollectionControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly RemoveModelFromCollectionController _controller;

    public RemoveModelFromCollectionControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When remove succeeds, RemoveModelFromCollection returns NoContent.")]
    public async Task RemoveModelFromCollection_Success_ReturnsNoContent()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var modelId = Guid.NewGuid();
        _mediator
            .Setup(m => m.Send(It.IsAny<RemoveModelFromCollectionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoveModelFromCollection(collectionId, modelId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
        _mediator.Verify(
            m => m.Send(
                It.Is<RemoveModelFromCollectionCommand>(c => c.CollectionId == collectionId && c.ModelId == modelId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "When the mediator throws, RemoveModelFromCollection propagates the exception.")]
    public async Task RemoveModelFromCollection_MediatorThrows_Propagates()
    {
        // Arrange
        _mediator
            .Setup(m => m.Send(It.IsAny<RemoveModelFromCollectionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("fail"));

        // Act
        var act = () => _controller.RemoveModelFromCollection(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
    }
}
