using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Categories.DeleteCategories.Domain;
using PolyBucket.Api.Features.Categories.DeleteCategories.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Categories.DeleteCategories;

public class DeleteCategoriesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly DeleteCategoriesController _controller;

    public DeleteCategoriesControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When category ids are provided, DeleteCategories sends the command and returns Ok.")]
    public async Task DeleteCategories_WithIds_ReturnsOk()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var response = new DeleteCategoriesResponse { DeletedCount = 2, FailedCount = 0 };
        _mediator.SetupSend<DeleteCategoriesCommand, DeleteCategoriesResponse>(response);

        // Act
        var result = await _controller.DeleteCategories(new DeleteCategoriesCommand { Ids = ids });

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        _mediator.Verify(m => m.Send(
            It.Is<DeleteCategoriesCommand>(c =>
                c.Ids.Count == 2 &&
                c.Ids[0] == ids[0] &&
                c.Ids[1] == ids[1]),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When no category ids are provided, DeleteCategories returns BadRequest.")]
    public async Task DeleteCategories_EmptyIds_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.DeleteCategories(new DeleteCategoriesCommand());

        // Assert
        var badRequest = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        badRequest.Value.ShouldBe("At least one category id is required");
        _mediator.Verify(
            m => m.Send(It.IsAny<DeleteCategoriesCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
