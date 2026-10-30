using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Categories.CreateCategory.Domain;
using PolyBucket.Api.Features.Categories.CreateCategory.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Categories.CreateCategory;

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
        var created = result.Result.ShouldBeOfType<CreatedAtRouteResult>();
        created.Value.ShouldBe(response);
        created.RouteName.ShouldBe("GetCategoryById");
    }
}
