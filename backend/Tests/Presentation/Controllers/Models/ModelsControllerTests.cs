using Api.Controllers.Models.Http;
using Conductors.Models;
using Core.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace Tests.Presentation.Controllers.Models
{
    public class ModelsControllerTests
    {
        private readonly Mock<IModelConductor> _mockModelConductor;
        private readonly Mock<ILogger<ModelsController>> _mockLogger;
        private readonly ModelsController _controller;

        public ModelsControllerTests()
        {
            _mockModelConductor = new Mock<IModelConductor>();
            _mockLogger = new Mock<ILogger<ModelsController>>();
            _controller = new ModelsController(_mockModelConductor.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetModels_ReturnsOkWithModels()
        {
            // Arrange
            var models = new List<Model>
            {
                new Model { Id = 1, Name = "Model 1" },
                new Model { Id = 2, Name = "Model 2" }
            };

            _mockModelConductor
                .Setup(x => x.GetModelsAsync())
                .ReturnsAsync(models);

            // Act
            var result = await _controller.GetModels();

            // Assert
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var returnedModels = okResult.Value.ShouldBeOfType<List<Model>>();
            returnedModels.Count.ShouldBe(2);
            returnedModels[0].Name.ShouldBe("Model 1");
            returnedModels[1].Name.ShouldBe("Model 2");
        }

        [Fact]
        public async Task GetModel_ExistingModel_ReturnsOkWithModel()
        {
            // Arrange
            var model = new Model { Id = 1, Name = "Test Model" };

            _mockModelConductor
                .Setup(x => x.GetModelByIdAsync(1))
                .ReturnsAsync(model);

            // Act
            var result = await _controller.GetModel(1);

            // Assert
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var returnedModel = okResult.Value.ShouldBeOfType<Model>();
            returnedModel.Id.ShouldBe(1);
            returnedModel.Name.ShouldBe("Test Model");
        }

        [Fact]
        public async Task GetModel_NonExistingModel_ReturnsNotFound()
        {
            // Arrange
            _mockModelConductor
                .Setup(x => x.GetModelByIdAsync(999))
                .ReturnsAsync((Model)null);

            // Act
            var result = await _controller.GetModel(999);

            // Assert
            var notFoundResult = result.Result.ShouldBeOfType<NotFoundObjectResult>();
            var response = notFoundResult.Value.ShouldBeOfType<object>();
            response.GetType().GetProperty("message").GetValue(response).ShouldBe("Model with ID 999 not found");
        }
    }
}