using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.Http;
using PolyBucket.Api.Features.Search.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Search;

public class SearchControllerUnitTests
{
    private readonly Mock<ISearchRepository> _repository = new();

    private SearchController CreateController() => new(_repository.Object);

    [Fact(DisplayName = "When the query is empty, search returns BadRequest.")]
    public async Task Search_EmptyQuery_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Search("   ", cancellationToken: CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _repository.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When the query exceeds 200 characters, search returns BadRequest.")]
    public async Task Search_QueryTooLong_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();
        var query = new string('a', 201);

        // Act
        var result = await controller.Search(query, cancellationToken: CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _repository.VerifyNoOtherCalls();
    }

    [Theory(DisplayName = "Invalid page or page size returns BadRequest.")]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task Search_InvalidPaging_ReturnsBadRequest(int page, int pageSize)
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Search("cats", page, pageSize, cancellationToken: CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _repository.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When the repository throws, search returns 500.")]
    public async Task Search_RepositoryError_Returns500()
    {
        // Arrange
        _repository
            .Setup(r => r.SearchAsync(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db"));
        var controller = CreateController();

        // Act
        var result = await controller.Search("cats", cancellationToken: CancellationToken.None);

        // Assert
        var status = result.Result.ShouldBeOfType<ObjectResult>();
        status.StatusCode.ShouldBe(500);
    }

    [Fact(DisplayName = "When the query is valid, search returns Ok with repository results.")]
    public async Task Search_Valid_ReturnsOk()
    {
        // Arrange
        var response = new SearchResponse { Query = "cats", TotalCount = 1, Page = 1, PageSize = 20 };
        _repository
            .Setup(r => r.SearchAsync(It.Is<SearchQuery>(q => q.Query == "cats"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = CreateController();

        // Act
        var result = await controller.Search("  cats  ", cancellationToken: CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
