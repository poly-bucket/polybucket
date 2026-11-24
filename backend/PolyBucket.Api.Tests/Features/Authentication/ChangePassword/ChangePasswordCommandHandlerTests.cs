using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Features.Authentication.ChangePassword.Domain;
using PolyBucket.Api.Features.Authentication.ChangePassword.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Database.Entities.ACL;
using PolyBucket.Database.Entities.Models;
using Shouldly;
using System.Security.Claims;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.ChangePassword;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IChangePasswordRepository> _repository = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IAccountEmailService> _email = new();
    private readonly Mock<IRecoveryCodePresentationService> _recoveryCodes = new();
    private readonly Mock<IPasswordPolicyService> _passwordPolicy = new();

    private ChangePasswordCommandHandler CreateHandler(HttpContext? httpContext = null)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);
        return new ChangePasswordCommandHandler(
            _repository.Object,
            _hasher.Object,
            _email.Object,
            _recoveryCodes.Object,
            _passwordPolicy.Object,
            NullLogger<ChangePasswordCommandHandler>.Instance,
            accessor.Object);
    }

    [Fact(DisplayName = "When the new password fails policy validation, change password returns failure without persisting.")]
    public async Task Handle_NewPasswordFailsPolicy_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())]));

        var user = new User
        {
            Id = userId,
            PasswordHash = "hash",
            Role = new Role { Name = "User" }
        };

        _repository.Setup(r => r.GetUserForPasswordChangeAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.VerifyPassword("OldPass1!", user.PasswordHash)).Returns(true);
        _passwordPolicy
            .Setup(p => p.ValidateAsync("short", It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Password must be at least 8 characters long."));
        var handler = CreateHandler(httpContext);

        // Act
        var response = await handler.Handle(new ChangePasswordCommand
        {
            CurrentPassword = "OldPass1!",
            NewPassword = "short",
            ConfirmPassword = "short"
        }, CancellationToken.None);

        // Assert
        response.Success.ShouldBeFalse();
        response.Message.ShouldContain("8");
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}