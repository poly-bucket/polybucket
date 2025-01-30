using Api.Controllers.Authentication.Http;
using Conductors.Login;
using Core.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace Tests.Presentation.Controllers.Authentication
{
    public class AuthControllerTests
    {
        private readonly Mock<ILoginConductor> _mockLoginConductor;
        private readonly Mock<ILogger<AuthenticationController>> _mockLogger;
        private readonly AuthenticationController _controller;

        public AuthControllerTests()
        {
            _mockLoginConductor = new Mock<ILoginConductor>();
            _mockLogger = new Mock<ILogger<AuthenticationController>>();
            _controller = new AuthenticationController(_mockLoginConductor.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var request = new CreateUserLoginRequest
            {
                Email = "test@example.com",
                Password = "password123",
                UserAgent = "test-agent"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Username = "testuser",
                FirstName = "Test",
                LastName = "User"
            };

            _mockLoginConductor
                .Setup(x => x.LoginAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(("test-token", user));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            var response = okResult.Value.ShouldBeOfType<LoginResponse>();
            response.Token.ShouldBe("test-token");
            response.User.Email.ShouldBe(request.Email);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new CreateUserLoginRequest
            {
                Email = "test@example.com",
                Password = "wrongpassword",
                UserAgent = "test-agent"
            };

            _mockLoginConductor
                .Setup(x => x.LoginAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = result.ShouldBeOfType<UnauthorizedObjectResult>();
            var response = unauthorizedResult.Value.ShouldBeOfType<object>();
            response.GetType().GetProperty("message").GetValue(response).ShouldBe("Invalid username or password");
        }
    }
}