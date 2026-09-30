using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.ResetPassword.Domain;
using PolyBucket.Api.Features.Authentication.ResetPassword.Http;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Http
{
    public class ResetPasswordCommandControllerTests
    {
        private readonly Mock<IAuthenticationRepository> _repository = new();
        private readonly Mock<IPasswordHasher> _hasher = new();
        private readonly ResetPasswordController _controller;

        public ResetPasswordCommandControllerTests()
        {
            _hasher.Setup(h => h.GenerateSalt()).Returns("salt");
            _hasher.Setup(h => h.HashPassword(It.IsAny<string>(), It.IsAny<string>())).Returns("hash");
            var handler = new ResetPasswordCommandHandler(
                _repository.Object,
                _hasher.Object,
                Mock.Of<IAccountEmailService>(),
                TimeProvider.System,
                NullLogger<ResetPasswordCommandHandler>.Instance);
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("198.51.100.4");
            _controller = new ResetPasswordController(handler, NullLogger<ResetPasswordController>.Instance)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };
        }

        private static ResetPasswordCommand Command() => new()
        {
            Token = "raw",
            NewPassword = "NewPassword1!",
            ConfirmPassword = "NewPassword1!"
        };

        private Guid SetupToken(DateTime expiresAt)
        {
            var token = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = TokenHasher.Hash("raw"),
                Email = "user@example.com",
                ExpiresAt = expiresAt
            };
            var userId = Guid.NewGuid();
            _repository.Setup(r => r.GetPasswordResetTokenByHashAsync(TokenHasher.Hash("raw"), It.IsAny<CancellationToken>())).ReturnsAsync(token);
            _repository.Setup(r => r.GetUserForUpdateByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = userId, Email = "user@example.com", Username = "user" });
            _repository.Setup(r => r.TryConsumePasswordResetTokenAsync(token.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            return userId;
        }

        [Fact(DisplayName = "When resetting a password with a valid token, the reset password controller returns Ok and revokes sessions using the caller IP.")]
        public async Task ResetPassword_ValidToken_ShouldReturnOk()
        {
            // Arrange
            var userId = SetupToken(DateTime.UtcNow.AddMinutes(30));

            // Act
            var result = await _controller.ResetPassword(Command(), CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            _repository.Verify(r => r.RevokeAllRefreshTokensForUserAsync(userId, It.IsAny<string>(), "198.51.100.4"), Times.Once);
        }

        [Fact(DisplayName = "When resetting a password with an invalid token, the reset password controller returns BadRequest.")]
        public async Task ResetPassword_InvalidToken_ShouldReturnBadRequest()
        {
            // Arrange
            var command = Command();

            // Act
            var result = await _controller.ResetPassword(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact(DisplayName = "When resetting a password with an expired token, the reset password controller returns BadRequest.")]
        public async Task ResetPassword_ExpiredToken_ShouldReturnBadRequest()
        {
            // Arrange
            SetupToken(DateTime.UtcNow.AddMinutes(-1));

            // Act
            var result = await _controller.ResetPassword(Command(), CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            _hasher.Verify(h => h.HashPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact(DisplayName = "When the model state is invalid, the reset password controller returns BadRequest.")]
        public async Task ResetPassword_InvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("NewPassword", "Too short");

            // Act
            var result = await _controller.ResetPassword(Command(), CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            _repository.VerifyNoOtherCalls();
        }

        [Fact(DisplayName = "The reset password endpoint uses the strict rate limiting policy.")]
        public void ResetPassword_ShouldUseStrictRateLimit()
        {
            // Arrange
            var method = typeof(ResetPasswordController).GetMethod(nameof(ResetPasswordController.ResetPassword));

            // Act
            var rateLimit = method!.GetCustomAttribute<EnableRateLimitingAttribute>();

            // Assert
            rateLimit!.PolicyName.ShouldBe(RequestSecurityServiceCollectionExtensions.AuthStrictPolicy);
        }
    }
}
