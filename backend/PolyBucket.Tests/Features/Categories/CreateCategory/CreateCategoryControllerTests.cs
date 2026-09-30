using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Categories.CreateCategory.Domain;
using PolyBucket.Api.Features.Categories.CreateCategory.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Categories.CreateCategory;

public class CreateCategoryControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    private CreateCategoryController CreateController() => new(_mediator.Object);

    [Fact(DisplayName = "When a category is created, CreateCategory returns 201 with the result.")]
    public async Task CreateCategory_Valid_ReturnsCreated()
    {
        // Arrange
        var command = new CreateCategoryCommand { Name = "Tools" };
        var response = new CreateCategoryResponse { Id = Guid.NewGuid(), Name = "Tools" };
        _mediator.SetupSend<CreateCategoryCommand, CreateCategoryResponse>(response);

        // Act
        var result = await CreateController().CreateCategory(command);

        // Assert
        var created = result.Result.ShouldBeOfType<CreatedAtActionResult>();
        created.Value.ShouldBe(response);
        created.ActionName.ShouldBe(nameof(CreateCategoryController.GetCategory));
    }

    [Fact(DisplayName = "When GetCategory finds a category, it returns Ok.")]
    public async Task GetCategory_Found_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new CreateCategoryResponse { Id = id, Name = "Tools" };
        _mediator.SetupSend<GetCategoryByIdQuery, CreateCategoryResponse?>(response);

        // Act
        var result = await CreateController().GetCategory(id);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }

    [Fact(DisplayName = "When GetCategory does not find a category, it returns NotFound.")]
    public async Task GetCategory_Missing_ReturnsNotFound()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<GetCategoryByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateCategoryResponse?)null);

        // Act
        var result = await CreateController().GetCategory(Guid.NewGuid());

        // Assert
        result.Result.ShouldBeOfType<NotFoundResult>();
    }
}
