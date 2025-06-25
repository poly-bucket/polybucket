using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.Queries;
using PolyBucket.Api.Features.Models.Domain;
using Shouldly;
using PolyBucket.Api.Features.Models.Repository;

namespace PolyBucket.Tests.Features.Models.Queries
{
    public class GetModelsQueryControllerTests
    {
        private readonly Mock<GetModelsQueryHandler> _mockHandler;
        private readonly Mock<ILogger<GetModelsQueryController>> _mockLogger;
        private readonly GetModelsQueryController _controller;

        public GetModelsQueryControllerTests()
        {
            // We need a mock repository to create a mock handler
            var mockRepo = new Mock<IModelsRepository>();
            _mockHandler = new Mock<GetModelsQueryHandler>(mockRepo.Object, null);
            _mockLogger = new Mock<ILogger<GetModelsQueryController>>();
            _controller = new GetModelsQueryController(_mockHandler.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetModels_ValidRequest_ReturnsPaginatedList()
        {
            // Arrange
            var request = new GetModelsRequest { Page = 1, Take = 20 };
            var models = new List<Model> { new Model(), new Model() };
            var expectedResponse = new GetModelsResponse
            {
                Models = models,
                TotalCount = 2,
                Page = 1,
                TotalPages = 1
            };

            _mockHandler.Setup(h => h.ExecuteAsync(It.IsAny<GetModelsRequest>())).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetModels(request);

            // Assert
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var response = okResult.Value.ShouldBeOfType<GetModelsResponse>();
            response.Models.Count().ShouldBe(2);
        }

        [Fact]
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

            _mockHandler.Setup(h => h.ExecuteAsync(It.IsAny<GetModelsRequest>())).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetModels(request);

            // Assert
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var response = okResult.Value.ShouldBeOfType<GetModelsResponse>();
            response.Models.ShouldBeEmpty();
        }
    }
} 