using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Collections.FavoriteCollection.Domain;
using PolyBucket.Api.Features.Collections.FavoriteCollection.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections.FavoriteCollection;

public class FavoriteCollectionControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly FavoriteCollectionController _controller;

    public FavoriteCollectionControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When route and body collection ids differ, ToggleFavorite returns BadRequest.")]
    public async Task ToggleFavorite_IdMismatch_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.ToggleFavorite(Guid.NewGuid(), new FavoriteCollectionCommand
        {
            CollectionId = Guid.NewGuid(),
            IsFavorite = true
        });

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
        _mediator.Verify(m => m.Send(It.IsAny<FavoriteCollectionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When favoriting succeeds, ToggleFavorite returns Ok.")]
    public async Task ToggleFavorite_Success_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new FavoriteCollectionResponse { Success = true, IsFavorite = true, Message = "Favorited" };
        _mediator.SetupSend<FavoriteCollectionCommand, FavoriteCollectionResponse>(response);

        // Act
        var result = await _controller.ToggleFavorite(id, new FavoriteCollectionCommand { CollectionId = id, IsFavorite = true });

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When the collection is not found, ToggleFavorite returns NotFound.")]
    public async Task ToggleFavorite_NotFound_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mediator.SetupSend<FavoriteCollectionCommand, FavoriteCollectionResponse>(
            new FavoriteCollectionResponse { Success = false, Message = "Collection not found" });

        // Act
        var result = await _controller.ToggleFavorite(id, new FavoriteCollectionCommand { CollectionId = id, IsFavorite = true });

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }
}
