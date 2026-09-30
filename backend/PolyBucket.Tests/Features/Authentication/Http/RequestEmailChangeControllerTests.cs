using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.RequestEmailChange.Domain;
using PolyBucket.Api.Features.Authentication.RequestEmailChange.Http;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Email.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Http;

public class RequestEmailChangeControllerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<IAuthenticationRepository> _repository = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IEmailSettingsResolver> _resolver = new();

    public RequestEmailChangeControllerTests()
    {
        _resolver.Setup(r => r.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveEmailSettings
            {
                Transport = EmailTransportKind.Log,
                FromAddress = "noreply@example.com",
                PublicBaseUrl = "https://models.example.com"
            });
        _repository.Setup(r => r.GetUserForUpdateByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = _userId, Email = "old@example.com", Username = "user", PasswordHash = "hash" });
        _hasher.Setup(h => h.VerifyPassword("correct", "hash")).Returns(true);
    }

    private RequestEmailChangeController CreateController(bool authenticated = true)
    {
        var handler = new RequestEmailChangeCommandHandler(
            _repository.Object,
            Mock.Of<ITokenService>(t => t.GenerateEmailVerificationToken() == "raw"),
            _hasher.Object,
            _resolver.Object,
            Mock.Of<IAccountEmailService>(),
            TimeProvider.System,
            NullLogger<RequestEmailChangeCommandHandler>.Instance);
        var identity = authenticated
            ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, _userId.ToString()) }, "Test")
            : new ClaimsIdentity();
        return new RequestEmailChangeController(handler)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            }
        };
    }

    [Fact(DisplayName = "When the password is correct, the email change controller returns Accepted.")]
    public async Task RequestEmailChange_Valid_ShouldReturnAccepted()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.RequestEmailChange(new RequestEmailChangeCommand { NewEmail = "new@example.com", CurrentPassword = "correct" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<AcceptedResult>();
    }

    [Fact(DisplayName = "When the password is wrong, the email change controller returns Unauthorized.")]
    public async Task RequestEmailChange_WrongPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.RequestEmailChange(new RequestEmailChangeCommand { NewEmail = "new@example.com", CurrentPassword = "wrong" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "When the new address is already taken, the email change controller returns BadRequest.")]
    public async Task RequestEmailChange_EmailTaken_ShouldReturnBadRequest()
    {
        // Arrange
        _repository.Setup(r => r.IsEmailTakenAsync("new@example.com")).ReturnsAsync(true);
        var controller = CreateController();

        // Act
        var result = await controller.RequestEmailChange(new RequestEmailChangeCommand { NewEmail = "new@example.com", CurrentPassword = "correct" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When the caller has no user id claim, the email change controller returns Unauthorized.")]
    public async Task RequestEmailChange_NoUserClaim_ShouldReturnUnauthorized()
    {
        // Arrange
        var controller = CreateController(authenticated: false);

        // Act
        var result = await controller.RequestEmailChange(new RequestEmailChangeCommand { NewEmail = "new@example.com", CurrentPassword = "correct" }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
    }

    [Fact(DisplayName = "The email change controller requires authentication.")]
    public void Controller_ShouldRequireAuthorization()
    {
        // Arrange
        var type = typeof(RequestEmailChangeController);

        // Act
        var authorize = type.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        authorize.ShouldNotBeNull();
    }
}
