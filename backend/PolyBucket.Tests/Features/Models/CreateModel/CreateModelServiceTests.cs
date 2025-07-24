using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.CreateModel.Http;
using PolyBucket.Api.Features.Models.CreateModel.Repository;
using PolyBucket.Api.Features.Models.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.CreateModel
{
    public class CreateModelServiceTests
    {
        private readonly Mock<ICreateModelRepository> _mockRepository;
        private readonly Mock<IStorageService> _mockStorage;
        private readonly Mock<ILogger<CreateModelService>> _mockLogger;
        private readonly CreateModelService _service;

        public CreateModelServiceTests()
        {
            _mockRepository = new Mock<ICreateModelRepository>();
            _mockStorage = new Mock<IStorageService>();
            _mockLogger = new Mock<ILogger<CreateModelService>>();
            _service = new CreateModelService(_mockRepository.Object, _mockStorage.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateModelAsync_WithValidRequest_ShouldCreateModel()
        {
            // Arrange
            var request = new CreateModelRequest
            {
                Name = "Test Model",
                Description = "Test Description",
                Privacy = "public",
                License = "mit",
                AIGenerated = false,
                WorkInProgress = false,
                NSFW = false,
                Remix = false,
                Files = CreateTestFiles()
            };

            var user = CreateTestUser();
            var cancellationToken = CancellationToken.None;

            _mockStorage.Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken))
                .ReturnsAsync("https://storage.example.com/test-file.stl");

            _mockRepository.Setup(x => x.CreateModelAsync(It.IsAny<Model>(), cancellationToken))
                .ReturnsAsync((Model model, CancellationToken ct) => model);

            // Act
            var result = await _service.CreateModelAsync(request, user, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Model.ShouldNotBeNull();
            result.Model.Name.ShouldBe("Test Model");
            result.Model.Description.ShouldBe("Test Description");
            
            _mockRepository.Verify(x => x.CreateModelAsync(It.IsAny<Model>(), cancellationToken), Times.Once);
            _mockStorage.Verify(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken), Times.AtLeastOnce);
        }

        [Fact]
        public async Task CreateModelAsync_WithEmptyName_ShouldThrowValidationException()
        {
            // Arrange
            var request = new CreateModelRequest
            {
                Name = "",
                Files = CreateTestFiles()
            };

            var user = CreateTestUser();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.CreateModelAsync(request, user, cancellationToken));
        }

        [Fact]
        public async Task CreateModelAsync_WithoutFiles_ShouldThrowValidationException()
        {
            // Arrange
            var request = new CreateModelRequest
            {
                Name = "Test Model",
                Files = new IFormFile[0]
            };

            var user = CreateTestUser();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.CreateModelAsync(request, user, cancellationToken));
        }

        [Fact]
        public async Task CreateModelAsync_WithInvalidUser_ShouldThrowValidationException()
        {
            // Arrange
            var request = new CreateModelRequest
            {
                Name = "Test Model",
                Files = CreateTestFiles()
            };

            var user = CreateInvalidUser();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.CreateModelAsync(request, user, cancellationToken));
        }

        private static IFormFile[] CreateTestFiles()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.stl");
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("application/octet-stream");
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1024]));

            return new[] { mockFile.Object };
        }

        private static ClaimsPrincipal CreateTestUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "test@example.com")
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        private static ClaimsPrincipal CreateInvalidUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "test@example.com")
                // Missing NameIdentifier claim
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }
    }
} 