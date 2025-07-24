using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Register.Domain;
using PolyBucket.Api.Features.Authentication.Register.Http;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Users.Domain;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using PolyBucket.Api.Data;

namespace PolyBucket.Tests.Features.Authentication.Http
{
    public class RegisterCommandControllerTests
    {
        private readonly Mock<IAuthenticationRepository> _authRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;
        private readonly Mock<PolyBucketDbContext> _contextMock;
        private readonly RegisterController _controller;
        private readonly RegisterCommandHandler _handler;

        public RegisterCommandControllerTests()
        {
            _authRepositoryMock = new Mock<IAuthenticationRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _emailServiceMock = new Mock<IEmailService>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<RegisterCommandHandler>>();
            _contextMock = new Mock<PolyBucketDbContext>();

            _handler = new RegisterCommandHandler(
                _authRepositoryMock.Object,
                _tokenServiceMock.Object,
                _emailServiceMock.Object,
                _passwordHasherMock.Object,
                _configurationMock.Object,
                _loggerMock.Object,
                _contextMock.Object);

            _controller = new RegisterController(_handler, Mock.Of<ILogger<RegisterController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task Register_ValidCommand_ShouldReturnOkWithAuthentication()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Country = "US",
                UserAgent = "Test User Agent"
            };

            var authResponse = new AuthenticationResponse
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                User = new UserInfo
                {
                    Id = Guid.NewGuid(),
                    Email = command.Email,
                    Username = command.Username,
                    Role = "User"
                }
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(It.IsAny<User>());
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(It.IsAny<User>()))
                .Returns(authResponse);
            _configurationMock.Setup(x => x["AppSettings:Email:RequireEmailVerification"])
                .Returns("false");
            _emailServiceMock.Setup(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value.ShouldBeOfType<RegisterCommandResponse>();
            response.Authentication.ShouldNotBeNull();
            response.Authentication.AccessToken.ShouldBe("test-access-token");
            response.RequiresEmailVerification.ShouldBeFalse();

            // Verify all expected calls were made
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(command.Username), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()), Times.Once);
            _emailServiceMock.Verify(x => x.SendWelcomeEmailAsync(command.Email, command.Username), Times.Once);
        }

        [Fact]
        public async Task Register_WithEmailVerificationEnabled_ShouldReturnOkWithVerificationToken()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            var authResponse = new AuthenticationResponse
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                User = new UserInfo { Id = Guid.NewGuid() }
            };

            var verificationToken = "test-verification-token";

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(It.IsAny<User>());
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);
            _authRepositoryMock.Setup(x => x.CreateEmailVerificationTokenAsync(It.IsAny<EmailVerificationToken>()))
                .ReturnsAsync(It.IsAny<EmailVerificationToken>());
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(It.IsAny<User>()))
                .Returns(authResponse);
            _tokenServiceMock.Setup(x => x.GenerateEmailVerificationToken())
                .Returns(verificationToken);
            _configurationMock.Setup(x => x["AppSettings:Email:RequireEmailVerification"])
                .Returns("true");
            _configurationMock.Setup(x => x["AppSettings:Frontend:BaseUrl"])
                .Returns("http://localhost:3000");
            _emailServiceMock.Setup(x => x.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value.ShouldBeOfType<RegisterCommandResponse>();
            response.RequiresEmailVerification.ShouldBeTrue();
            response.EmailVerificationToken.ShouldBe(verificationToken);

            // Verify verification email was sent
            _emailServiceMock.Verify(x => x.SendEmailVerificationAsync(
                command.Email, 
                verificationToken, 
                "http://localhost:3000/verify-email"), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateEmailVerificationTokenAsync(It.IsAny<EmailVerificationToken>()), Times.Once);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ShouldReturnConflict()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "existing@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ConflictObjectResult>();
            var conflictResult = (ConflictObjectResult)result;
            conflictResult.Value.ShouldNotBeNull();

            // Verify only email check was made
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(It.IsAny<string>()), Times.Never);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Register_DuplicateUsername_ShouldReturnConflict()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "existinguser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ConflictObjectResult>();
            var conflictResult = (ConflictObjectResult)result;
            conflictResult.Value.ShouldNotBeNull();

            // Verify both checks were made but no user created
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(command.Username), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Register_InvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "invalid-email",
                Username = "",
                Password = "short",
                ConfirmPassword = "different",
                UserAgent = "Test User Agent"
            };

            _controller.ModelState.AddModelError("Email", "Invalid email format");
            _controller.ModelState.AddModelError("Username", "Username is required");
            _controller.ModelState.AddModelError("Password", "Password too short");
            _controller.ModelState.AddModelError("ConfirmPassword", "Passwords do not match");

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.Value.ShouldNotBeNull();

            // Verify no repository calls were made
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(It.IsAny<string>()), Times.Never);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(It.IsAny<string>()), Times.Never);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Register_RepositoryThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.ShouldBe(500);
            errorResult.Value.ShouldNotBeNull();

            // Verify only the first check was attempted
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(It.IsAny<string>()), Times.Never);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Register_TokenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(It.IsAny<User>());
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(It.IsAny<User>()))
                .Throws(new Exception("Token generation failed"));

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.ShouldBe(500);
            errorResult.Value.ShouldNotBeNull();

            // Verify user was created but token generation failed
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(command.Username), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateAuthenticationResponse(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Register_EmailServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                UserAgent = "Test User Agent"
            };

            var authResponse = new AuthenticationResponse
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                User = new UserInfo { Id = Guid.NewGuid() }
            };

            _authRepositoryMock.Setup(x => x.IsEmailTakenAsync(command.Email))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.IsUsernameTakenAsync(command.Username))
                .ReturnsAsync(false);
            _authRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(It.IsAny<User>());
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(It.IsAny<User>()))
                .Returns(authResponse);
            _configurationMock.Setup(x => x["AppSettings:Email:RequireEmailVerification"])
                .Returns("false");
            _emailServiceMock.Setup(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service failed"));

            // Act
            var result = await _controller.Register(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.ShouldBe(500);
            errorResult.Value.ShouldNotBeNull();

            // Verify user was created and token generated but email failed
            _authRepositoryMock.Verify(x => x.IsEmailTakenAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.IsUsernameTakenAsync(command.Username), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateAuthenticationResponse(It.IsAny<User>()), Times.Once);
            _emailServiceMock.Verify(x => x.SendWelcomeEmailAsync(command.Email, command.Username), Times.Once);
        }
    }
} 