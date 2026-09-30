using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Collections.UpdateCollection.Domain;
using PolyBucket.Api.Features.Collections.UpdateCollection.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;
using CollectionEntity = PolyBucket.Api.Features.Collections.Domain.Collection;

namespace PolyBucket.Tests.Features.Collections.UpdateCollection;

public class UpdateCollectionControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly UpdateCollectionController _controller;

    public UpdateCollectionControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When route and body ids differ, UpdateCollection returns BadRequest.")]
    public async Task UpdateCollection_IdMismatch_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UpdateCollection(Guid.NewGuid(), new UpdateCollectionCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated"
        });

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
        _mediator.Verify(m => m.Send(It.IsAny<UpdateCollectionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When update succeeds, UpdateCollection returns Ok with the collection.")]
    public async Task UpdateCollection_Success_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updated = new CollectionEntity { Id = id, Name = "Updated" };
        _mediator.SetupSend<UpdateCollectionCommand, CollectionEntity>(updated);

        // Act
        var result = await _controller.UpdateCollection(id, new UpdateCollectionCommand { Id = id, Name = "Updated" });

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(updated);
    }

    [Fact(DisplayName = "When the mediator throws, UpdateCollection propagates the exception.")]
    public async Task UpdateCollection_MediatorThrows_Propagates()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mediator.SetupSendThrows<UpdateCollectionCommand, CollectionEntity>(new InvalidOperationException("fail"));

        // Act
        var act = () => _controller.UpdateCollection(id, new UpdateCollectionCommand { Id = id, Name = "x" });

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
    }
}
