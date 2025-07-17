using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Enums;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.RefreshToken.Domain;
using PolyBucket.Api.Features.Authentication.RefreshToken.Http;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using RefreshTokenModel = PolyBucket.Api.Features.Authentication.Domain.RefreshToken;

namespace PolyBucket.Tests.Features.Authentication.Http
{
    public class RefreshTokenCommandControllerTests
    {
        private readonly Mock<IAuthenticationRepository> _authRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock;
        private readonly RefreshTokenController _controller;
        private readonly RefreshTokenCommandHandler _handler;

        public RefreshTokenCommandControllerTests()
        {
            _authRepositoryMock = new Mock<IAuthenticationRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _loggerMock = new Mock<ILogger<RefreshTokenCommandHandler>>();

            _handler = new RefreshTokenCommandHandler(
                _authRepositoryMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object);

            _controller = new RefreshTokenController(_handler, Mock.Of<ILogger<RefreshTokenController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task RefreshToken_ValidToken_ShouldReturnOkWithNewTokens()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid-refresh-token"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var validRefreshToken = new RefreshTokenModel
            {
                Id = Guid.NewGuid(),
                Token = command.RefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                CreatedByIp = "127.0.0.1",
                RevokedAt = null,
                User = user
            };

            var newAuthResponse = new AuthenticationResponse
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    Role = user.Role.ToString()
                }
            };

            _authRepositoryMock.Setup(x => x.GetRefreshTokenAsync(command.RefreshToken))
                .ReturnsAsync(validRefreshToken);
            _authRepositoryMock.Setup(x => x.GetUserByEmailAsync(user.Email))
                .ReturnsAsync(user);
            _authRepositoryMock.Setup(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(user))
                .Returns(newAuthResponse);
            _authRepositoryMock.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshTokenModel>()))
                .ReturnsAsync(It.IsAny<RefreshTokenModel>());

            // Act
            var result = await _controller.RefreshToken(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value.ShouldBeOfType<RefreshTokenCommandResponse>();
            response.Authentication.ShouldNotBeNull();
            response.Authentication.AccessToken.ShouldBe("new-access-token");
            response.Authentication.RefreshToken.ShouldBe("new-refresh-token");

            // Verify all expected calls were made
            _authRepositoryMock.Verify(x => x.GetRefreshTokenAsync(command.RefreshToken), Times.Once);
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(user.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.RevokeRefreshTokenAsync(command.RefreshToken, "Replaced by new token", "127.0.0.1"), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateAuthenticationResponse(user), Times.Once);
            _authRepositoryMock.Verify(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshTokenModel>()), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_InvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "invalid-refresh-token"
            };

            _authRepositoryMock.Setup(x => x.GetRefreshTokenAsync(command.RefreshToken))
                .ReturnsAsync((RefreshTokenModel?)null);

            // Act
            var result = await _controller.RefreshToken(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = (UnauthorizedObjectResult)result;
            unauthorizedResult.Value.ShouldNotBeNull();

            // Verify only token lookup was made
            _authRepositoryMock.Verify(x => x.GetRefreshTokenAsync(command.RefreshToken), Times.Once);
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
            _authRepositoryMock.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_ExpiredToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "expired-refresh-token"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                Role = UserRole.User
            };

            var expiredRefreshToken = new RefreshTokenModel
            {
                Id = Guid.NewGuid(),
                Token = command.RefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired yesterday
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                CreatedByIp = "127.0.0.1",
                RevokedAt = null,
                User = user
            };

            _authRepositoryMock.Setup(x => x.GetRefreshTokenAsync(command.RefreshToken))
                .ReturnsAsync(expiredRefreshToken);

            // Act
            var result = await _controller.RefreshToken(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = (UnauthorizedObjectResult)result;
            unauthorizedResult.Value.ShouldNotBeNull();

            // Verify only token lookup was made
            _authRepositoryMock.Verify(x => x.GetRefreshTokenAsync(command.RefreshToken), Times.Once);
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_RevokedToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "revoked-refresh-token"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                Role = UserRole.User
            };

            var revokedRefreshToken = new RefreshTokenModel
            {
                Id = Guid.NewGuid(),
                Token = command.RefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                CreatedByIp = "127.0.0.1",
                RevokedAt = DateTime.UtcNow.AddMinutes(-5), // Revoked 5 minutes ago
                RevokedByIp = "127.0.0.1",
                ReasonRevoked = "Manual revocation",
                User = user
            };

            _authRepositoryMock.Setup(x => x.GetRefreshTokenAsync(command.RefreshToken))
                .ReturnsAsync(revokedRefreshToken);

            // Act
            var result = await _controller.RefreshToken(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = (UnauthorizedObjectResult)result;
            unauthorizedResult.Value.ShouldNotBeNull();

            // Verify only token lookup was made
            _authRepositoryMock.Verify(x => x.GetRefreshTokenAsync(command.RefreshToken), Times.Once);
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_UserNotFound_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid-refresh-token"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                Role = UserRole.User
            };

            var validRefreshToken = new RefreshTokenModel
            {
                Id = Guid.NewGuid(),
                Token = command.RefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                CreatedByIp = "127.0.0.1",
                RevokedAt = null,
                User = user
            };

            _authRepositoryMock.Setup(x => x.GetRefreshTokenAsync(command.RefreshToken))
                .ReturnsAsync(validRefreshToken);
            _authRepositoryMock.Setup(x => x.GetUserByEmailAsync(user.Email))
                .ReturnsAsync((User?)null); // User not found

            // Act
            var result = await _controller.RefreshToken(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = (UnauthorizedObjectResult)result;
            unauthorizedResult.Value.ShouldNotBeNull();

            // Verify calls were made up to the point of failure
            _authRepositoryMock.Verify(x => x.GetRefreshTokenAsync(command.RefreshToken), Times.Once);
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(user.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_InvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "" // Invalid empty token
            };

            _controller.ModelState.AddModelError("RefreshToken", "Refresh token is required");

            // Act
            var result = await _controller.RefreshToken(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.Value.ShouldNotBeNull();

            // Verify no repository calls were made
            _authRepositoryMock.Verify(x => x.GetRefreshTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_RepositoryThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid-refresh-token"
            };

            _authRepositoryMock.Setup(x => x.GetRefreshTokenAsync(command.RefreshToken))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.RefreshToken(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.ShouldBe(500);
            errorResult.Value.ShouldNotBeNull();

            // Verify repository was called but other operations were not
            _authRepositoryMock.Verify(x => x.GetRefreshTokenAsync(command.RefreshToken), Times.Once);
            _authRepositoryMock.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_TokenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid-refresh-token"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                Role = UserRole.User
            };

            var validRefreshToken = new RefreshTokenModel
            {
                Id = Guid.NewGuid(),
                Token = command.RefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                CreatedByIp = "127.0.0.1",
                RevokedAt = null,
                User = user
            };

            _authRepositoryMock.Setup(x => x.GetRefreshTokenAsync(command.RefreshToken))
                .ReturnsAsync(validRefreshToken);
            _authRepositoryMock.Setup(x => x.GetUserByEmailAsync(user.Email))
                .ReturnsAsync(user);
            _authRepositoryMock.Setup(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _tokenServiceMock.Setup(x => x.GenerateAuthenticationResponse(user))
                .Throws(new Exception("Token generation failed"));

            // Act
            var result = await _controller.RefreshToken(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result;
            errorResult.StatusCode.ShouldBe(500);
            errorResult.Value.ShouldNotBeNull();

            // Verify calls were made up to the point of failure
            _authRepositoryMock.Verify(x => x.GetRefreshTokenAsync(command.RefreshToken), Times.Once);
            _authRepositoryMock.Verify(x => x.GetUserByEmailAsync(user.Email), Times.Once);
            _authRepositoryMock.Verify(x => x.RevokeRefreshTokenAsync(command.RefreshToken, "Replaced by new token", "127.0.0.1"), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateAuthenticationResponse(user), Times.Once);
        }
    }
} 