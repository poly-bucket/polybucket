using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.ResendVerificationEmail.Domain;
using PolyBucket.Api.Features.Authentication.ResendVerificationEmail.Http;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Http;

public class ResendVerificationEmailControllerTests
{
    private readonly Mock<IAuthenticationRepository> _repository = new();
    private readonly Mock<IEmailSettingsResolver> _resolver = new();
    private readonly ResendVerificationEmailController _controller;

    public ResendVerificationEmailControllerTests()
    {
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveEmailSettings { Transport = EmailTransportKind.Log, FromAddress = "noreply@example.com" });
        var handler = new ResendVerificationEmailCommandHandler(
            _repository.Object,
            Mock.Of<ITokenService>(),
            _resolver.Object,
            Mock.Of<IAccountEmailService>(),
            TimeProvider.System,
            NullLogger<ResendVerificationEmailCommandHandler>.Instance);
        _controller = new ResendVerificationEmailController(handler, NullLogger<ResendVerificationEmailController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact(DisplayName = "When resending for an unknown address, the controller returns Accepted with the generic message.")]
    public async Task Resend_UnknownEmail_ShouldReturnAccepted()
    {
        // Arrange
        var command = new ResendVerificationEmailCommand { Email = "nobody@example.com" };

        // Act
        var result = await _controller.Resend(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<AcceptedResult>();
    }

    [Fact(DisplayName = "When the handler throws, the controller still returns Accepted so account existence is not revealed.")]
    public async Task Resend_HandlerThrows_ShouldReturnAccepted()
    {
        // Arrange
        _repository.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ThrowsAsync(new Exception("db down"));

        // Act
        var result = await _controller.Resend(new ResendVerificationEmailCommand { Email = "user@example.com" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<AcceptedResult>();
    }

    [Fact(DisplayName = "When the model state is invalid, the controller returns BadRequest.")]
    public async Task Resend_InvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Email", "Invalid");

        // Act
        var result = await _controller.Resend(new ResendVerificationEmailCommand(), CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "The resend endpoint allows anonymous callers and uses the strict rate limit.")]
    public void Resend_ShouldBeAnonymousAndStrictlyRateLimited()
    {
        // Arrange
        var method = typeof(ResendVerificationEmailController).GetMethod(nameof(ResendVerificationEmailController.Resend));

        // Act
        var anonymous = typeof(ResendVerificationEmailController).GetCustomAttribute<AllowAnonymousAttribute>();
        var rateLimit = method!.GetCustomAttribute<EnableRateLimitingAttribute>();

        // Assert
        anonymous.ShouldNotBeNull();
        rateLimit!.PolicyName.ShouldBe(RequestSecurityServiceCollectionExtensions.AuthStrictPolicy);
    }
}
