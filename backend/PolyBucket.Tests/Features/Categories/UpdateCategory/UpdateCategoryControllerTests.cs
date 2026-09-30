using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Categories.UpdateCategory.Domain;
using PolyBucket.Api.Features.Categories.UpdateCategory.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Categories.UpdateCategory;

public class UpdateCategoryControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly UpdateCategoryController _controller;

    public UpdateCategoryControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When route id and body id differ, UpdateCategory returns BadRequest.")]
    public async Task UpdateCategory_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var command = new UpdateCategoryCommand { Id = Guid.NewGuid(), Name = "A" };

        // Act
        var result = await _controller.UpdateCategory(Guid.NewGuid(), command);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _mediator.Verify(m => m.Send(It.IsAny<UpdateCategoryCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When the update succeeds, UpdateCategory returns Ok with the response.")]
    public async Task UpdateCategory_Valid_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateCategoryCommand { Id = id, Name = "Updated" };
        var response = new UpdateCategoryResponse { Id = id, Name = "Updated" };
        _mediator.SetupSend<UpdateCategoryCommand, UpdateCategoryResponse>(response);

        // Act
        var result = await _controller.UpdateCategory(id, command);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
