using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Categories.DeleteCategory.Domain;
using PolyBucket.Api.Features.Categories.DeleteCategory.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Categories.DeleteCategory;

public class DeleteCategoryControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly DeleteCategoryController _controller;

    public DeleteCategoryControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When deletion succeeds, DeleteCategory returns Ok with the response.")]
    public async Task DeleteCategory_Valid_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new DeleteCategoryResponse { Id = id, Success = true, Message = "Deleted" };
        _mediator.SetupSend<DeleteCategoryCommand, DeleteCategoryResponse>(response);

        // Act
        var result = await _controller.DeleteCategory(id);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        _mediator.Verify(m => m.Send(
            It.Is<DeleteCategoryCommand>(c => c.Id == id),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
