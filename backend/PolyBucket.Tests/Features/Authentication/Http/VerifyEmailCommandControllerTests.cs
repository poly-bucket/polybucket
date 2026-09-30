using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Authentication.VerifyEmail.Domain;
using PolyBucket.Api.Features.Authentication.VerifyEmail.Http;
using PolyBucket.Api.Features.Email.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Http
{
    public class VerifyEmailCommandControllerTests
    {
        private readonly Mock<IAuthenticationRepository> _repository = new();
        private readonly VerifyEmailController _controller;

        public VerifyEmailCommandControllerTests()
        {
            var handler = new VerifyEmailCommandHandler(
                _repository.Object,
                Mock.Of<IAccountEmailService>(),
                TimeProvider.System,
                NullLogger<VerifyEmailCommandHandler>.Instance);
            _controller = new VerifyEmailController(handler, NullLogger<VerifyEmailController>.Instance)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

        private void SetupToken(DateTime expiresAt, bool consumed = true)
        {
            var token = new EmailVerificationToken
            {
                Id = Guid.NewGuid(),
                Token = TokenHasher.Hash("raw"),
                Email = "user@example.com",
                ExpiresAt = expiresAt
            };
            _repository.Setup(r => r.GetEmailVerificationTokenByHashAsync(TokenHasher.Hash("raw"), It.IsAny<CancellationToken>())).ReturnsAsync(token);
            _repository.Setup(r => r.GetUserForUpdateByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "user@example.com", Username = "user" });
            _repository.Setup(r => r.TryConsumeEmailVerificationTokenAsync(token.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(consumed);
        }

        [Fact(DisplayName = "When verifying an email with a valid token, the verify email controller returns Ok.")]
        public async Task VerifyEmail_ValidToken_ShouldReturnOk()
        {
            // Arrange
            SetupToken(DateTime.UtcNow.AddHours(1));

            // Act
            var result = await _controller.VerifyEmail(new VerifyEmailCommand { Token = "raw" }, CancellationToken.None);

            // Assert
            var ok = result.ShouldBeOfType<OkObjectResult>();
            var response = ok.Value.ShouldBeOfType<VerifyEmailResponse>();
            response.Purpose.ShouldBe(EmailVerificationPurpose.VerifyAddress);
            response.Email.ShouldBe("user@example.com");
        }

        [Fact(DisplayName = "When verifying an email with an invalid token, the verify email controller returns BadRequest.")]
        public async Task VerifyEmail_InvalidToken_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new VerifyEmailCommand { Token = "unknown" };

            // Act
            var result = await _controller.VerifyEmail(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact(DisplayName = "When verifying an email with an expired token, the verify email controller returns BadRequest.")]
        public async Task VerifyEmail_ExpiredToken_ShouldReturnBadRequest()
        {
            // Arrange
            SetupToken(DateTime.UtcNow.AddMinutes(-1));

            // Act
            var result = await _controller.VerifyEmail(new VerifyEmailCommand { Token = "raw" }, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact(DisplayName = "When the same verification link is used twice, the verify email controller returns BadRequest the second time.")]
        public async Task VerifyEmail_ReusedToken_ShouldReturnBadRequest()
        {
            // Arrange
            SetupToken(DateTime.UtcNow.AddHours(1), consumed: false);

            // Act
            var result = await _controller.VerifyEmail(new VerifyEmailCommand { Token = "raw" }, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact(DisplayName = "When the model state is invalid, the verify email controller returns BadRequest without calling the handler.")]
        public async Task VerifyEmail_InvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Token", "Required");

            // Act
            var result = await _controller.VerifyEmail(new VerifyEmailCommand(), CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            _repository.VerifyNoOtherCalls();
        }

        [Fact(DisplayName = "The verify email endpoint is rate limited.")]
        public void VerifyEmail_ShouldBeRateLimited()
        {
            // Arrange
            var method = typeof(VerifyEmailController).GetMethod(nameof(VerifyEmailController.VerifyEmail));

            // Act
            var rateLimit = method!.GetCustomAttribute<EnableRateLimitingAttribute>();

            // Assert
            rateLimit.ShouldNotBeNull();
        }
    }
}
