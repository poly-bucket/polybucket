using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Collections.CreateCollection.Domain;
using PolyBucket.Api.Features.Collections.CreateCollection.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;
using CollectionEntity = PolyBucket.Api.Features.Collections.Domain.Collection;

namespace PolyBucket.Tests.Features.Collections.CreateCollection;

public class CreateCollectionControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly CreateCollectionController _controller;

    public CreateCollectionControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When creation succeeds, CreateCollection returns Ok with the collection.")]
    public async Task CreateCollection_Success_ReturnsOk()
    {
        // Arrange
        var command = new CreateCollectionCommand { Name = "Benchies" };
        var created = new CollectionEntity { Id = Guid.NewGuid(), Name = "Benchies" };
        _mediator.SetupSend<CreateCollectionCommand, CollectionEntity>(created);

        // Act
        var result = await _controller.CreateCollection(command);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(created);
    }

    [Fact(DisplayName = "When the mediator throws, CreateCollection propagates the exception.")]
    public async Task CreateCollection_MediatorThrows_Propagates()
    {
        // Arrange
        _mediator.SetupSendThrows<CreateCollectionCommand, CollectionEntity>(new InvalidOperationException("fail"));

        // Act
        var act = () => _controller.CreateCollection(new CreateCollectionCommand { Name = "x" });

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
    }
}
