using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.Queries;
using PolyBucket.Api.Features.Users.Domain;
using Microsoft.AspNetCore.Http;

namespace PolyBucket.Tests.Features.Users.Queries
{
    public class GetUserSettingsQueryControllerTests
    {
        private readonly Mock<GetUserSettingsQueryHandler> _mockHandler;
        private readonly Mock<ILogger<GetUserSettingsQueryController>> _mockLogger;
        private readonly GetUserSettingsQueryController _controller;
        private readonly Guid _testUserId;

        public GetUserSettingsQueryControllerTests()
        {
            // Mock the handler directly. We'll test the handler's logic in a separate integration test.
            _mockHandler = new Mock<GetUserSettingsQueryHandler>(null, null); // DbContext and Logger are mocked as null
            _mockLogger = new Mock<ILogger<GetUserSettingsQueryController>>();

            _controller = new GetUserSettingsQueryController(_mockHandler.Object, _mockLogger.Object);

            _testUserId = Guid.NewGuid();
            var claims = new List<Claim> { new Claim("sub", _testUserId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetUserSettings_WhenSettingsExist_ReturnsOkResultWithSettings()
        {
            // Arrange
            var userSettings = new UserSettings { UserId = _testUserId, Language = "en" };
            var response = new GetUserSettingsResponse { Settings = userSettings };

            _mockHandler.Setup(h => h.ExecuteAsync(It.Is<GetUserSettingsRequest>(r => r.UserId == _testUserId)))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetUserSettings();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResponse = Assert.IsType<GetUserSettingsResponse>(okResult.Value);
            Assert.Equal(_testUserId, returnedResponse.Settings.UserId);
        }

        [Fact]
        public async Task GetUserSettings_WhenSettingsDoNotExist_ReturnsNotFound()
        {
            // Arrange
            _mockHandler.Setup(h => h.ExecuteAsync(It.IsAny<GetUserSettingsRequest>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetUserSettings();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUserSettings_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            _mockHandler.Setup(h => h.ExecuteAsync(It.IsAny<GetUserSettingsRequest>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetUserSettings();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
} 