using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
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
        private readonly Mock<ICreateUserService> _serviceMock;
        private readonly Mock<ILogger<CreateUserController>> _loggerMock;
        private readonly CreateUserController _controller;

        public CreateUserControllerTests()
        {
            _serviceMock = new Mock<ICreateUserService>();
            _loggerMock = new Mock<ILogger<CreateUserController>>();
            _controller = new CreateUserController(_serviceMock.Object, _loggerMock.Object);

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

        [Fact(DisplayName = "When creating a user with valid data, the create user controller returns Created with the new user.")]
        public async Task CreateUser_WithValidData_ShouldReturnCreatedResult()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                RoleId = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Country = "US"
            };

            var expectedResponse = new CreateUserCommandResponse
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                Username = command.Username,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Country = command.Country,
                GeneratedPassword = "TempPassword123!",
                CreatedAt = DateTime.UtcNow,
                EmailVerificationRequired = false
            };

            _serviceMock.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
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

        [Fact(DisplayName = "When creating a user with an email that is already registered, the create user controller returns Conflict.")]
        public async Task CreateUser_WithDuplicateEmail_ShouldReturnConflict()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "existing@example.com",
                Username = "testuser",
                RoleId = Guid.NewGuid()
            };

            _serviceMock.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Email is already registered"));

            // Act
            var result = await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact(DisplayName = "When creating a user with a username that is already taken, the create user controller returns Conflict.")]
        public async Task CreateUser_WithDuplicateUsername_ShouldReturnConflict()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Username = "existinguser",
                RoleId = Guid.NewGuid()
            };

            _serviceMock.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Username is already taken"));

            // Act
            var result = await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact(DisplayName = "When creating a user with an invalid model, the create user controller returns BadRequest.")]
        public async Task CreateUser_WithInvalidModel_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "", // Invalid email
                Username = "testuser",
                RoleId = Guid.NewGuid()
            };

            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact(DisplayName = "When creating a user and the service throws an unexpected error, the create user controller returns InternalServerError.")]
        public async Task CreateUser_WithUnexpectedError_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                RoleId = Guid.NewGuid()
            };

            _serviceMock.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact(DisplayName = "When creating a user with a specific role id, the create user controller forwards the role id and returns Created.")]
        public async Task CreateUser_WithDifferentRoleIds_ShouldSucceed()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                RoleId = roleId
            };

            var expectedResponse = new CreateUserCommandResponse
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                Username = command.Username,
                GeneratedPassword = "TempPassword123!",
                CreatedAt = DateTime.UtcNow,
                EmailVerificationRequired = false
            };

            _serviceMock.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var responseData = Assert.IsType<CreateUserCommandResponse>(createdResult.Value);
            Assert.Equal(command.RoleId, roleId);
        }

        [Fact(DisplayName = "When creating a user, the create user controller forwards the user agent and IP from the request to the service.")]
        public async Task CreateUser_ShouldSetUserAgentAndIpFromRequest()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                RoleId = Guid.NewGuid()
            };

            var expectedResponse = new CreateUserCommandResponse
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                Username = command.Username,
                GeneratedPassword = "TempPassword123!",
                CreatedAt = DateTime.UtcNow,
                EmailVerificationRequired = false
            };

            CreateUserCommand? capturedCommand = null;
            _serviceMock.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .Callback<CreateUserCommand, CancellationToken>((cmd, _) => capturedCommand = cmd)
                .ReturnsAsync(expectedResponse);

            // Act
            await _controller.CreateUser(command, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedCommand);
            Assert.Equal("Test User Agent", capturedCommand!.UserAgent);
            Assert.Equal("127.0.0.1", capturedCommand.CreatedByIp);
        }
    }
} 