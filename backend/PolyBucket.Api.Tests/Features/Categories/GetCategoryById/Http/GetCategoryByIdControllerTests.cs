using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Categories.GetCategoryById.Domain;
using PolyBucket.Api.Features.Categories.GetCategoryById.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Categories.GetCategoryById.Http;

public class GetCategoryByIdControllerTests
{
    private readonly Mock<IGetCategoryByIdService> _service = new();

    [Fact(DisplayName = "When the category exists, GetCategoryById returns Ok.")]
    public async Task GetCategoryById_Found_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new GetCategoryByIdResponseDto { Id = id, Name = "Props" };
        _service.Setup(s => s.GetCategoryByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var controller = new GetCategoryByIdController(_service.Object);

        // Act
        var result = await controller.GetCategoryById(id, CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(dto);
    }

    [Fact(DisplayName = "When the category is missing, GetCategoryById returns NotFound.")]
    public async Task GetCategoryById_Missing_ReturnsNotFound()
    {
        // Arrange
        _service.Setup(s => s.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCategoryByIdResponseDto?)null);
        var controller = new GetCategoryByIdController(_service.Object);

        // Act
        var result = await controller.GetCategoryById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundResult>();
    }
}
