using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Controllers;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Shouldly;
using Xunit;
using File = Core.Entities.File;

namespace PolyBucket.Tests.Controllers
{
    public class ModelsControllerTests : IDisposable
    {
        protected readonly Mock<IModelService> _mockModelService;
        protected readonly Mock<IStorageService> _mockStorageService;
        protected readonly Mock<ISystemSetupRepository> _mockSystemSetupRepository;
        protected readonly ModelsController _controller;

        public ModelsControllerTests()
        {
            _mockModelService = new Mock<IModelService>();
            _mockStorageService = new Mock<IStorageService>();
            _mockSystemSetupRepository = new Mock<ISystemSetupRepository>();
            
            _controller = new ModelsController(
                _mockModelService.Object,
                _mockStorageService.Object,
                _mockSystemSetupRepository.Object);
            
            // Set up HttpContext for controller
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact(DisplayName = "GetModelById Returns Model When Found (200)")]
        public async Task GetModelById_ExistingModel_ReturnsModel()
        {
            // Arrange
            var model = CreateTestModel();
            
            // Setup with explicit cancellation token to avoid optional parameters
            _mockSystemSetupRepository
                .Setup(repo => repo.RequireUploadModerationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockModelService
                .Setup(service => service.GetModelByIdAsync(model.Id))
                .ReturnsAsync(model);

            // Act
            var result = await _controller.GetModelById(model.Id);

            // Assert
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var response = okResult.Value.ShouldBeOfType<Model>();
            response.ShouldBe(model);
        }

        [Fact(DisplayName = "GetModelById Returns NotFound When Model Not Found (404)")]
        public async Task GetModelById_NonExistingModel_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            
            _mockModelService
                .Setup(service => service.GetModelByIdAsync(nonExistingId))
                .ReturnsAsync((Model)null);

            // Act
            var result = await _controller.GetModelById(nonExistingId);

            // Assert
            result.Result.ShouldBeOfType<NotFoundResult>();
        }

        private Model CreateTestModel()
        {
            return new Model
            {
                Id = Guid.NewGuid(),
                Name = "Test Model",
                Description = "Test Description",
                UserId = Guid.NewGuid(),
                License = "CC BY-SA",
                ThumbnailUrl = "https://example.com/thumbnail.jpg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ModerationStatus = ModerationStatus.Approved
            };
        }

        public void Dispose()
        {
            _mockModelService.VerifyAll();
            _mockStorageService.VerifyAll();
            _mockSystemSetupRepository.VerifyAll();
        }
    }
} 