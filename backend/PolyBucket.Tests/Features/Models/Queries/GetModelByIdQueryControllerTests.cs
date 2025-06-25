using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.Queries;
using PolyBucket.Api.Features.Models.Domain;
using Shouldly;
using PolyBucket.Api.Features.Models.Repository;
using System.Collections.Generic;

namespace PolyBucket.Tests.Features.Models.Queries
{
    public class GetModelByIdQueryControllerTests
    {
        private readonly Mock<GetModelByIdQueryHandler> _mockHandler;
        private readonly Mock<ILogger<GetModelByIdQueryController>> _mockLogger;
        private readonly GetModelByIdQueryController _controller;

        public GetModelByIdQueryControllerTests()
        {
            var mockRepo = new Mock<IModelsRepository>();
            _mockHandler = new Mock<GetModelByIdQueryHandler>(mockRepo.Object, null);
            _mockLogger = new Mock<ILogger<GetModelByIdQueryController>>();
            _controller = new GetModelByIdQueryController(_mockHandler.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetModelById_ExistingModel_ReturnsModel()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var model = new Model { Id = modelId, Name = "Test Model" };
            var expectedResponse = new GetModelByIdResponse { Model = model };

            _mockHandler.Setup(h => h.ExecuteAsync(It.Is<GetModelByIdRequest>(r => r.Id == modelId)))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetModel(modelId);

            // Assert
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var response = okResult.Value.ShouldBeOfType<GetModelByIdResponse>();
            response.Model.Id.ShouldBe(modelId);
        }

        [Fact]
        public async Task GetModelById_NonExistingModel_ReturnsNotFound()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            _mockHandler.Setup(h => h.ExecuteAsync(It.Is<GetModelByIdRequest>(r => r.Id == modelId)))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetModel(modelId);

            // Assert
            result.Result.ShouldBeOfType<NotFoundObjectResult>();
        }
    }
} 