using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Enums;
using PolyBucket.Api.Features.Users.CreateUser.Domain;
using PolyBucket.Api.Features.Users.CreateUser.Http;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Features.Users.CreateUser.Http
{
    public class CreateUserControllerTests
    {
        private readonly Mock<CreateUserCommandHandler> _handlerMock;
        private readonly Mock<ILogger<CreateUserController>> _loggerMock;
        private readonly CreateUserController _controller;

        public CreateUserControllerTests()
        {
            _handlerMock = new Mock<CreateUserCommandHandler>();
            _loggerMock = new Mock<ILogger<CreateUserController>>();
            _controller = new CreateUserController(_handlerMock.Object, _loggerMock.Object);

            // Setup HttpContext with mock user claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-user-id"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim("role", "Admin")
            }));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user,
                    Connection = { RemoteIpAddress = System.Net.IPAddress.Loopback }
                }
            };

            _controller.ControllerContext.HttpContext.Request.Headers["User-Agent"] = "Test User Agent";
        }

        [Fact]
        public async Task CreateUser_WithValidData_ShouldReturnCreatedResult()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Role = UserRole.User,
                FirstName = "Test",
                LastName = "User",
                Country = "US"
            };

            var expectedResponse = new CreateUserCommandResponse
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                Username = command.Username,
                Role = command.Role,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Country = command.Country,
                GeneratedPassword = "TempPassword123!",
                CreatedAt = DateTime.UtcNow,
                EmailVerificationRequired = false
            };

            _handlerMock.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            
            var responseData = Assert.IsType<CreateUserCommandResponse>(createdResult.Value);
            Assert.Equal(expectedResponse.Email, responseData.Email);
            Assert.Equal(expectedResponse.Username, responseData.Username);
            Assert.Equal(expectedResponse.GeneratedPassword, responseData.GeneratedPassword);
        }

        [Fact]
        public async Task CreateUser_WithDuplicateEmail_ShouldReturnConflict()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "existing@example.com",
                Username = "testuser",
                Role = UserRole.User
            };

            _handlerMock.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Email is already registered"));

            // Act
            var result = await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact]
        public async Task CreateUser_WithDuplicateUsername_ShouldReturnConflict()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Username = "existinguser",
                Role = UserRole.User
            };

            _handlerMock.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Username is already taken"));

            // Act
            var result = await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact]
        public async Task CreateUser_WithInvalidModel_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "", // Invalid email
                Username = "testuser",
                Role = UserRole.User
            };

            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateUser_WithUnexpectedError_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Role = UserRole.User
            };

            _handlerMock.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Theory]
        [InlineData(UserRole.User)]
        [InlineData(UserRole.Moderator)]
        [InlineData(UserRole.Admin)]
        public async Task CreateUser_WithDifferentRoles_ShouldSucceed(UserRole role)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Role = role
            };

            var expectedResponse = new CreateUserCommandResponse
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                Username = command.Username,
                Role = role,
                GeneratedPassword = "TempPassword123!",
                CreatedAt = DateTime.UtcNow,
                EmailVerificationRequired = false
            };

            _handlerMock.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var responseData = Assert.IsType<CreateUserCommandResponse>(createdResult.Value);
            Assert.Equal(role, responseData.Role);
        }

        [Fact]
        public async Task CreateUser_ShouldSetUserAgentAndIpFromRequest()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Role = UserRole.User
            };

            var expectedResponse = new CreateUserCommandResponse
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                Username = command.Username,
                Role = command.Role,
                GeneratedPassword = "TempPassword123!",
                CreatedAt = DateTime.UtcNow,
                EmailVerificationRequired = false
            };

            CreateUserCommand capturedCommand = null;
            _handlerMock.Setup(x => x.Handle(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .Callback<CreateUserCommand, CancellationToken>((cmd, _) => capturedCommand = cmd)
                .ReturnsAsync(expectedResponse);

            // Act
            await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedCommand);
            Assert.Equal("Test User Agent", capturedCommand.UserAgent);
            Assert.Equal("127.0.0.1", capturedCommand.CreatedByIp);
        }
    }
} 