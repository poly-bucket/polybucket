using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Collections.AddModelToCollection.Domain;
using PolyBucket.Api.Features.Collections.AddModelToCollection.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections.AddModelToCollection;

public class AddModelToCollectionControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly AddModelToCollectionController _controller;

    public AddModelToCollectionControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When add succeeds, AddModelToCollection returns NoContent.")]
    public async Task AddModelToCollection_Success_ReturnsNoContent()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var modelId = Guid.NewGuid();
        _mediator
            .Setup(m => m.Send(It.IsAny<AddModelToCollectionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddModelToCollection(collectionId, modelId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
        _mediator.Verify(
            m => m.Send(
                It.Is<AddModelToCollectionCommand>(c => c.CollectionId == collectionId && c.ModelId == modelId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "When the mediator throws, AddModelToCollection propagates the exception.")]
    public async Task AddModelToCollection_MediatorThrows_Propagates()
    {
        // Arrange
        _mediator
            .Setup(m => m.Send(It.IsAny<AddModelToCollectionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("fail"));

        // Act
        var act = () => _controller.AddModelToCollection(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
    }
}
