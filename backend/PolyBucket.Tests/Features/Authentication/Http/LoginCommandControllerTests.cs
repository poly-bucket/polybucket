using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using PolyBucket.Api.Features.Authentication.Login.Http;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using PolyBucket.Api.Features.ACL.Domain;
using RefreshTokenModel = PolyBucket.Api.Features.Authentication.Domain.RefreshToken;

namespace PolyBucket.Tests.Features.Authentication.Http
{
    public class LoginCommandControllerTests
    {
        private readonly Mock<IAuthenticationRepository> _authRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;
        private readonly LoginController _controller;
        private readonly LoginCommandHandler _handler;

        public LoginCommandControllerTests()
        {
            _authRepositoryMock = new Mock<IAuthenticationRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<LoginCommandHandler>>();
            
            _handler = new LoginCommandHandler(
                _authRepositoryMock.Object,
                _tokenServiceMock.Object,
                _passwordHasherMock.Object,
                _configurationMock.Object,
                _loggerMock.Object,
                null!); // Pass null for DbContext in tests
                
            _controller = new LoginController(_handler, Mock.Of<ILogger<LoginController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task Login_ValidCredentials_ShouldReturnOkWithToken()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password123",
                UserAgent = "Test User Agent"
            };

            var userRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "User",
                Description = "Default user role"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                RoleId = userRole.Id,
                Role = userRole,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var authResponse = new AuthenticationResponse
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    Role = user.Role?.Name ?? "User"
                }
            };

            _authRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(true);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(user))
                .Returns(authResponse);
            _authRepositoryMock.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshTokenModel>()))
                .ReturnsAsync(It.IsAny<RefreshTokenModel>());
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);
            
            // Note: The database context mock will return null for SystemSetups by default
            // This is sufficient for the tests since we're not testing the setup functionality

            // Act
            var result = await _controller.Login(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value.ShouldBeOfType<LoginCommandResponse>();
            response.Token.ShouldBe("test-access-token");

            // Verify all expected calls were made
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(command.Email), Times.Once);
            _passwordHasherMock.Verify(x => x.VerifyPassword(command.Password, user.PasswordHash), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshTokenModel>()), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()), Times.Once); // One successful login record
            _tokenServiceMock.Verify(x => x.GenerateAuthenticationResponse(user), Times.Once);
        }

        [Fact]
        public async Task Login_InvalidEmail_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "nonexistent@example.com",
                Password = "password123",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
                .ReturnsAsync((User?)null);
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Login(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = (UnauthorizedObjectResult)result;
            unauthorizedResult.Value.ShouldNotBeNull();

            // Verify failed login was logged
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshTokenModel>()), Times.Never);
        }

        [Fact]
        public async Task Login_InvalidPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "wrongpassword",
                UserAgent = "Test User Agent"
            };

            var userRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "User",
                Description = "Default user role"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                RoleId = userRole.Id,
                Role = userRole,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _authRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(false);
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Login(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = (UnauthorizedObjectResult)result;
            unauthorizedResult.Value.ShouldNotBeNull();

            // Verify failed login was logged but no token created
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(command.Email), Times.Once);
            _passwordHasherMock.Verify(x => x.VerifyPassword(command.Password, user.PasswordHash), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshTokenModel>()), Times.Never);
            _tokenServiceMock.Verify(x => x.GenerateAuthenticationResponse(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_RepositoryThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password123",
                UserAgent = "Test User Agent"
            };

            _authRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.Login(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.ShouldBe(500);
            errorResult.Value.ShouldNotBeNull();

            // Verify repository was called but other operations were not
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(command.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshTokenModel>()), Times.Never);
            _tokenServiceMock.Verify(x => x.GenerateAuthenticationResponse(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_TokenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "test@example.com",
                Password = "password123",
                UserAgent = "Test User Agent"
            };

            var userRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "User",
                Description = "Default user role"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                RoleId = userRole.Id,
                Role = userRole,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _authRepositoryMock.Setup(x => x.GetUserByEmailAsync(command.Email))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
                .Returns(true);
            _authRepositoryMock.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(user))
                .Throws(new Exception("Token generation failed"));

            // Act
            var result = await _controller.Login(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.ShouldBe(500);
            errorResult.Value.ShouldNotBeNull();

            // Verify all expected calls were made
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(command.Email), Times.Once);
            _passwordHasherMock.Verify(x => x.VerifyPassword(command.Password, user.PasswordHash), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateAuthenticationResponse(user), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshTokenModel>()), Times.Never);
        }

        [Fact]
        public async Task Login_EmptyCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "",
                Password = "",
                UserAgent = "Test User Agent"
            };

            // Act
            var result = await _controller.Login(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = (UnauthorizedObjectResult)result;
            unauthorizedResult.Value.ShouldNotBeNull();

            // Verify no repository calls were made
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
            _authRepositoryMock.Verify(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshTokenModel>()), Times.Never);
            _tokenServiceMock.Verify(x => x.GenerateAuthenticationResponse(It.IsAny<User>()), Times.Never);
        }
    }
} 