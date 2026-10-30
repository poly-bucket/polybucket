using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.SearchModels.Domain;
using PolyBucket.Api.Features.Search.SearchModels.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Search.SearchModels.Http;

public class SearchModelsControllerTests
{
    private readonly Mock<ISearchModelsService> _service = new();

    [Fact(DisplayName = "When the query is empty, SearchModels returns BadRequest.")]
    public async Task SearchModels_EmptyQuery_ReturnsBadRequest()
    {
        // Arrange
        var controller = new SearchModelsController(_service.Object);

        // Act
        var result = await controller.SearchModels("   ", cancellationToken: CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
        _service.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When the query is valid, SearchModels returns Ok.")]
    public async Task SearchModels_Valid_ReturnsOk()
    {
        // Arrange
        var response = new SearchResponse { Query = "cat", TotalCount = 1 };
        _service.Setup(s => s.SearchModelsAsync(It.Is<SearchQuery>(q => q.Query == "cat"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = new SearchModelsController(_service.Object);

        // Act
        var result = await controller.SearchModels("cat", cancellationToken: CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
