using Api.Controllers.Models.Domain;
using Api.Controllers.Models.Http;
using Api.Controllers.Models.Persistance;
using Core.Models.Models;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Tests.Factories;

namespace Tests.Api.Tests.Controllers.Models;

public class ModelsControllerTests : IDisposable
{
    private readonly Mock<IGetModelsService> _mockGetModelsService;
    private readonly Mock<IGetModelByIdService> _mockGetModelByIdService;
    private readonly Mock<ILogger<ModelsController>> _mockLogger;
    private readonly ModelsController _controller;
    private readonly Context _context;
    private readonly TestModelFactory _testModelFactory;

    public ModelsControllerTests()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new Context(options);

        _mockGetModelsService = new Mock<IGetModelsService>();
        _mockGetModelByIdService = new Mock<IGetModelByIdService>();
        _mockLogger = new Mock<ILogger<ModelsController>>();
        
        _controller = new ModelsController(
            _mockGetModelsService.Object,
            _mockGetModelByIdService.Object,
            _mockLogger.Object);

        _testModelFactory = new TestModelFactory(_context);
    }

    [Fact(DisplayName = "GetModels Returns Paginated List (200)")]
    public async Task GetModels_ValidRequest_ReturnsPaginatedList()
    {
        // Arrange
        var request = new GetModelsRequest { Page = 1, Take = 20 };
        var models = await _testModelFactory.CreateTestModels(5);
        var expectedResponse = new GetModelsResponse
        {
            Models = models,
            TotalCount = 5,
            Page = 1,
            TotalPages = 1
        };

        _mockGetModelsService
            .Setup(x => x.ExecuteAsync(It.IsAny<GetModelsRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetModels(request);

        // Assert
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<GetModelsResponse>();
        response.Models.Count().ShouldBe(5);
        response.TotalCount.ShouldBe(5);
        response.Page.ShouldBe(1);
        response.TotalPages.ShouldBe(1);
    }

    [Fact(DisplayName = "GetModels Returns Empty List When No Models (200)")]
    public async Task GetModels_NoModels_ReturnsEmptyList()
    {
        // Arrange
        var request = new GetModelsRequest { Page = 1, Take = 20 };
        var expectedResponse = new GetModelsResponse
        {
            Models = new List<Model>(),
            TotalCount = 0,
            Page = 1,
            TotalPages = 0
        };

        _mockGetModelsService
            .Setup(x => x.ExecuteAsync(It.IsAny<GetModelsRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetModels(request);

        // Assert
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<GetModelsResponse>();
        response.Models.Count().ShouldBe(0);
        response.TotalCount.ShouldBe(0);
        response.Page.ShouldBe(1);
        response.TotalPages.ShouldBe(0);
    }

    [Fact(DisplayName = "GetModelById Returns Model When Found (200)")]
    public async Task GetModelById_ExistingModel_ReturnsModel()
    {
        // Arrange
        var model = (await _testModelFactory.CreateTestModels(1)).First();
        var expectedResponse = new GetModelByIdResponse { Model = model };

        _mockGetModelByIdService
            .Setup(x => x.ExecuteAsync(It.IsAny<GetModelByIdRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetModel(model.Id);

        // Assert
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<GetModelByIdResponse>();
        response.Model.ShouldNotBeNull();
        response.Model.Id.ShouldBe(model.Id);
    }

    [Fact(DisplayName = "GetModelById Returns NotFound When Model Not Found (404)")]
    public async Task GetModelById_NonExistingModel_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        _mockGetModelByIdService
            .Setup(x => x.ExecuteAsync(It.IsAny<GetModelByIdRequest>()))
            .ThrowsAsync(new KeyNotFoundException($"Model with ID {nonExistingId} not found"));

        // Act
        var result = await _controller.GetModel(nonExistingId);

        // Assert
        var notFoundResult = result.Result.ShouldBeOfType<NotFoundObjectResult>();
        var errorResponse = notFoundResult.Value;
        errorResponse.GetType().GetProperty("message").GetValue(errorResponse).ToString()
            .ShouldBe($"Model with ID {nonExistingId} not found");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 