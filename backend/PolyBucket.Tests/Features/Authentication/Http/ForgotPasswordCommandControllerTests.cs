using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.ForgotPassword.Domain;
using PolyBucket.Api.Features.Authentication.ForgotPassword.Http;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Http
{
    public class ForgotPasswordCommandControllerTests
    {
        private readonly Mock<IAuthenticationRepository> _repository = new();
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly Mock<IAccountEmailService> _accountEmail = new();
        private readonly ForgotPasswordController _controller;

        public ForgotPasswordCommandControllerTests()
        {
            _tokenService.Setup(t => t.GeneratePasswordResetToken()).Returns("raw-reset-token");
            var handler = new ForgotPasswordCommandHandler(
                _repository.Object,
                _tokenService.Object,
                _accountEmail.Object,
                NullLogger<ForgotPasswordCommandHandler>.Instance);
            _controller = new ForgotPasswordController(handler, NullLogger<ForgotPasswordController>.Instance)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

        [Fact(DisplayName = "When sending a forgot password request with a valid email, the forgot password controller returns Ok and stores only the token hash.")]
        public async Task ForgotPassword_ValidEmail_ShouldReturnOk()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", Username = "user" };
            _repository.Setup(r => r.GetUserByEmailAsync("user@example.com")).ReturnsAsync(user);

            // Act
            var result = await _controller.ForgotPassword(new ForgotPasswordCommand { Email = "user@example.com" }, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            _repository.Verify(r => r.CreatePasswordResetTokenAsync(It.Is<PasswordResetToken>(t =>
                t.Token == TokenHasher.Hash("raw-reset-token") && t.Email == "user@example.com")), Times.Once);
            _accountEmail.Verify(a => a.SendPasswordResetAsync(user, "raw-reset-token", ForgotPasswordCommandHandler.ResetTokenLifetime, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When sending a forgot password request for an unknown email, the forgot password controller still returns Ok.")]
        public async Task ForgotPassword_UnknownEmail_ShouldReturnOkWithoutSending()
        {
            // Arrange
            var command = new ForgotPasswordCommand { Email = "nobody@example.com" };

            // Act
            var result = await _controller.ForgotPassword(command, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            _accountEmail.VerifyNoOtherCalls();
        }

        [Fact(DisplayName = "When sending a forgot password request with an invalid email, the forgot password controller returns BadRequest.")]
        public async Task ForgotPassword_InvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Invalid email");

            // Act
            var result = await _controller.ForgotPassword(new ForgotPasswordCommand { Email = "not-an-email" }, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            _repository.VerifyNoOtherCalls();
        }

        [Fact(DisplayName = "When the handler fails, the forgot password controller still returns Ok so account existence is not revealed.")]
        public async Task ForgotPassword_HandlerThrows_ShouldReturnOk()
        {
            // Arrange
            _repository.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ThrowsAsync(new Exception("db down"));

            // Act
            var result = await _controller.ForgotPassword(new ForgotPasswordCommand { Email = "user@example.com" }, CancellationToken.None);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
        }
    }
}
