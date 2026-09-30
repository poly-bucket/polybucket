using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Categories.GetCategories.Domain;
using PolyBucket.Api.Features.Categories.GetCategories.Http;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Categories.GetCategories;

public class GetCategoriesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly GetCategoriesController _controller;

    public GetCategoriesControllerTests() => _controller = new(_mediator.Object);

    [Fact(DisplayName = "When categories are requested, GetCategories sends the query and returns Ok.")]
    public async Task GetCategories_ReturnsOk()
    {
        // Arrange
        var response = new GetCategoriesResponse { TotalCount = 1, Page = 1, PageSize = 20 };
        _mediator.SetupSend<GetCategoriesQuery, GetCategoriesResponse>(response);

        // Act
        var result = await _controller.GetCategories(1, 20, "tool");

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
        _mediator.Verify(m => m.Send(
            It.Is<GetCategoriesQuery>(q => q.Page == 1 && q.PageSize == 20 && q.SearchTerm == "tool"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
