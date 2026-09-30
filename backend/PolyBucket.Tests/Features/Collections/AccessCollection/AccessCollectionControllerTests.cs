using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Collections.AccessCollection.Domain;
using PolyBucket.Api.Features.Collections.AccessCollection.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;
using CollectionEntity = PolyBucket.Api.Features.Collections.Domain.Collection;

namespace PolyBucket.Tests.Features.Collections.AccessCollection;

public class AccessCollectionControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly AccessCollectionController _controller;

    public AccessCollectionControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When route and body collection ids differ, AccessCollection returns BadRequest.")]
    public async Task AccessCollection_IdMismatch_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.AccessCollection(Guid.NewGuid(), new AccessCollectionCommand
        {
            CollectionId = Guid.NewGuid()
        });

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When access succeeds, AccessCollection returns Ok with the collection.")]
    public async Task AccessCollection_Success_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var collection = new CollectionEntity { Id = id, Name = "Secret" };
        _mediator.SetupSend<AccessCollectionCommand, AccessCollectionResponse>(
            new AccessCollectionResponse { Success = true, Collection = collection });

        // Act
        var result = await _controller.AccessCollection(id, new AccessCollectionCommand { CollectionId = id });

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(collection);
    }

    [Fact(DisplayName = "When a password is required, AccessCollection returns Unauthorized.")]
    public async Task AccessCollection_RequiresPassword_ReturnsUnauthorized()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mediator.SetupSend<AccessCollectionCommand, AccessCollectionResponse>(
            new AccessCollectionResponse { Success = false, RequiresPassword = true, Message = "Password required" });

        // Act
        var result = await _controller.AccessCollection(id, new AccessCollectionCommand { CollectionId = id });

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "When the collection is not found, AccessCollection returns NotFound.")]
    public async Task AccessCollection_NotFound_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mediator.SetupSend<AccessCollectionCommand, AccessCollectionResponse>(
            new AccessCollectionResponse { Success = false, Message = "Not found" });

        // Act
        var result = await _controller.AccessCollection(id, new AccessCollectionCommand { CollectionId = id });

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
