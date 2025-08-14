using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Users.UpdateUserProfile.Domain;
using PolyBucket.Api.Features.Users.UpdateUserProfile.Handlers;
using PolyBucket.Api.Features.Users.UpdateUserProfile.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Features.Users.UpdateUserProfile
{
    public class UpdateUserProfileControllerTests
    {
        private readonly Mock<UpdateUserProfileCommandHandler> _mockHandler;
        private readonly Mock<ILogger<UpdateUserProfileController>> _mockLogger;
        private readonly UpdateUserProfileController _controller;

        public UpdateUserProfileControllerTests()
        {
            _mockHandler = new Mock<UpdateUserProfileCommandHandler>();
            _mockLogger = new Mock<ILogger<UpdateUserProfileController>>();
            _controller = new UpdateUserProfileController(_mockHandler.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task UpdateUserProfile_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new UpdateUserProfileRequest
            {
                Bio = "Updated bio",
                Country = "Canada",
                WebsiteUrl = "https://example.com",
                TwitterUrl = "https://twitter.com/testuser",
                IsProfilePublic = true,
                ShowEmail = false,
                ShowLastLogin = true,
                ShowStatistics = true
            };

            _mockHandler.Setup(h => h.Handle(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Mock the User.FindFirst to return a valid user ID
            var mockUser = new Mock<System.Security.Claims.ClaimsPrincipal>();
            mockUser.Setup(u => u.FindFirst(It.IsAny<string>())).Returns(new Mock<System.Security.Claims.Claim>("sub", Guid.NewGuid().ToString()).Object);
            
            // Act
            var result = await _controller.UpdateUserProfile(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Anonymous>(okResult.Value);
            Assert.Equal("User profile updated successfully", response.message);
        }

        [Fact]
        public async Task UpdateUserProfile_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new UpdateUserProfileRequest
            {
                Bio = "Updated bio",
                Country = "Canada"
            };

            _mockHandler.Setup(h => h.Handle(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            // Mock the User.FindFirst to return a valid user ID
            var mockUser = new Mock<System.Security.Claims.ClaimsPrincipal>();
            mockUser.Setup(u => u.FindFirst(It.IsAny<string>())).Returns(new Mock<System.Security.Claims.Claim>("sub", Guid.NewGuid().ToString()).Object);

            // Act
            var result = await _controller.UpdateUserProfile(request, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = Assert.IsType<Anonymous>(notFoundResult.Value);
            Assert.Equal("User not found", errorResponse.message);
        }

        [Fact]
        public async Task UpdateUserProfile_UnauthorizedAccess_ReturnsForbid()
        {
            // Arrange
            var request = new UpdateUserProfileRequest
            {
                Bio = "Updated bio"
            };

            _mockHandler.Setup(h => h.Handle(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException("Unauthorized access"));

            // Mock the User.FindFirst to return a valid user ID
            var mockUser = new Mock<System.Security.Claims.ClaimsPrincipal>();
            mockUser.Setup(u => u.FindFirst(It.IsAny<string>())).Returns(new Mock<System.Security.Claims.Claim>("sub", Guid.NewGuid().ToString()).Object);

            // Act
            var result = await _controller.UpdateUserProfile(request, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateUserProfile_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var request = new UpdateUserProfileRequest
            {
                Bio = "Updated bio"
            };

            _mockHandler.Setup(h => h.Handle(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Mock the User.FindFirst to return a valid user ID
            var mockUser = new Mock<System.Security.Claims.ClaimsPrincipal>();
            mockUser.Setup(u => u.FindFirst(It.IsAny<string>())).Returns(new Mock<System.Security.Claims.Claim>("sub", Guid.NewGuid().ToString()).Object);

            // Act
            var result = await _controller.UpdateUserProfile(request, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var errorResponse = Assert.IsType<Anonymous>(statusCodeResult.Value);
            Assert.Equal("An error occurred while updating the user profile", errorResponse.message);
        }

        [Fact]
        public async Task UpdateUserProfile_InvalidUserId_ReturnsUnauthorized()
        {
            // Arrange
            var request = new UpdateUserProfileRequest
            {
                Bio = "Updated bio"
            };

            // Mock the User.FindFirst to return null (invalid user ID)
            var mockUser = new Mock<System.Security.Claims.ClaimsPrincipal>();
            mockUser.Setup(u => u.FindFirst(It.IsAny<string>())).Returns((System.Security.Claims.Claim)null);

            // Act
            var result = await _controller.UpdateUserProfile(request, CancellationToken.None);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }

    public class Anonymous
    {
        public string message { get; set; } = string.Empty;
    }
}
