using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.Queries.GetUserProfile;
using PolyBucket.Api.Features.Users.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace PolyBucket.Tests.Features.Users.GetUserProfile
{
    public class GetUserProfileControllerTests
    {
        private readonly Mock<GetUserProfileQueryHandler> _mockHandler;
        private readonly Mock<ILogger<GetUserProfileController>> _mockLogger;
        private readonly GetUserProfileController _controller;

        public GetUserProfileControllerTests()
        {
            _mockHandler = new Mock<GetUserProfileQueryHandler>();
            _mockLogger = new Mock<ILogger<GetUserProfileController>>();
            _controller = new GetUserProfileController(_mockHandler.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUserProfileById_ValidId_ReturnsOkResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedResponse = new GetUserProfileResponse
            {
                Id = userId,
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Bio = "Test bio",
                Avatar = "avatar.jpg",
                Country = "US",
                RoleName = "User",
                CreatedAt = DateTime.UtcNow,
                TotalModels = 5,
                TotalCollections = 2,
                TotalLikes = 10,
                TotalDownloads = 25
            };

            _mockHandler.Setup(h => h.Handle(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserProfileById(userId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<GetUserProfileResponse>(okResult.Value);
            Assert.Equal(userId, response.Id);
            Assert.Equal("testuser", response.Username);
            Assert.Equal(5, response.TotalModels);
        }

        [Fact]
        public async Task GetUserProfileById_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockHandler.Setup(h => h.Handle(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            // Act
            var result = await _controller.GetUserProfileById(userId, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var errorResponse = Assert.IsType<Anonymous>(notFoundResult.Value);
            Assert.Equal("User profile not found", errorResponse.message);
        }

        [Fact]
        public async Task GetUserProfileById_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockHandler.Setup(h => h.Handle(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetUserProfileById(userId, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var errorResponse = Assert.IsType<Anonymous>(statusCodeResult.Value);
            Assert.Equal("An error occurred while retrieving the user profile", errorResponse.message);
        }

        [Fact]
        public async Task GetUserProfileByUsername_ValidUsername_ReturnsOkResult()
        {
            // Arrange
            var username = "testuser";
            var expectedResponse = new GetUserProfileResponse
            {
                Id = Guid.NewGuid(),
                Username = username,
                FirstName = "Test",
                LastName = "User",
                Bio = "Test bio",
                Avatar = "avatar.jpg",
                Country = "US",
                RoleName = "User",
                CreatedAt = DateTime.UtcNow,
                TotalModels = 3,
                TotalCollections = 1,
                TotalLikes = 8,
                TotalDownloads = 15
            };

            _mockHandler.Setup(h => h.Handle(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserProfileByUsername(username, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<GetUserProfileResponse>(okResult.Value);
            Assert.Equal(username, response.Username);
            Assert.Equal(3, response.TotalModels);
        }

        [Fact]
        public async Task GetUserProfileByUsername_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var username = "nonexistentuser";
            _mockHandler.Setup(h => h.Handle(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            // Act
            var result = await _controller.GetUserProfileByUsername(username, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var errorResponse = Assert.IsType<Anonymous>(notFoundResult.Value);
            Assert.Equal("User profile not found", errorResponse.message);
        }

        [Fact]
        public async Task GetUserProfileByUsername_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var username = "testuser";
            _mockHandler.Setup(h => h.Handle(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetUserProfileByUsername(username, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var errorResponse = Assert.IsType<Anonymous>(statusCodeResult.Value);
            Assert.Equal("An error occurred while retrieving the user profile", errorResponse.message);
        }
    }

    public class Anonymous
    {
        public string message { get; set; } = string.Empty;
    }
}
