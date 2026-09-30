using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Authentication.Account.Domain;
using PolyBucket.Api.Features.Authentication.Account.Http;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using PolyBucket.Api.Features.Authentication.Login.Repository;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Account.Http;

public class DeleteAccountControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly Guid _userId;
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly Mock<IAuthenticationRepository> _authRepository = new();
    private readonly Mock<ILoginTwoFactorAuthService> _loginTwoFactorAuthService = new();
    private readonly Mock<ILoginTwoFactorAuthRepository> _loginTwoFactorAuthRepository = new();

    public DeleteAccountControllerTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        _userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _context.Roles.Add(new Role { Id = roleId, Name = "User", Description = "User", IsActive = true });
        _context.Users.Add(new User
        {
            Id = _userId,
            Email = "delete@test.com",
            Username = "deleteuser",
            Salt = "s",
            PasswordHash = "h",
            RoleId = roleId,
            Role = _context.Roles.Find(roleId),
            CanLogin = true
        });
        _context.SaveChanges();

        _permissionService
            .Setup(p => p.HasPermissionAsync(_userId, PermissionConstants.USER_DELETE_ACCOUNT))
            .ReturnsAsync(true);
        _passwordHasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), "h")).Returns(true);
        _passwordHasher.Setup(h => h.GenerateSalt()).Returns("salt");
        _passwordHasher.Setup(h => h.HashPassword(It.IsAny<string>(), It.IsAny<string>())).Returns("hash");
        _loginTwoFactorAuthRepository.Setup(r => r.GetByUserIdAsync(_userId)).ReturnsAsync((PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth?)null);
        _authRepository
            .Setup(r => r.RevokeAllRefreshTokensForUserAsync(_userId, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private DeleteAccountController CreateController(Guid? userId)
    {
        var service = new DeleteOwnAccountService(
            _context,
            _authRepository.Object,
            _passwordHasher.Object,
            _loginTwoFactorAuthService.Object,
            _loginTwoFactorAuthRepository.Object,
            _permissionService.Object,
            NullLogger<DeleteOwnAccountService>.Instance);
        return new DeleteAccountController(service).WithUser(userId);
    }

    [Fact(DisplayName = "When password is missing, DeleteAccount returns BadRequest.")]
    public async Task DeleteAccount_MissingPassword_ReturnsBadRequest()
    {
        // Act
        var result = await CreateController(_userId).DeleteAccount(new DeleteAccountRequest { Password = " " }, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
        _passwordHasher.Verify(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "When deletion succeeds, DeleteAccount returns Ok.")]
    public async Task DeleteAccount_Success_ReturnsOk()
    {
        // Act
        var result = await CreateController(_userId).DeleteAccount(
            new DeleteAccountRequest { Password = "correct" },
            CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeOfType<DeleteAccountResponse>().Success.ShouldBeTrue();
        (await _context.Users.FindAsync(_userId))!.CanLogin.ShouldBeFalse();
    }

    [Fact(DisplayName = "When the password is wrong, DeleteAccount returns BadRequest.")]
    public async Task DeleteAccount_WrongPassword_ReturnsBadRequest()
    {
        // Arrange
        _passwordHasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), "h")).Returns(false);

        // Act
        var result = await CreateController(_userId).DeleteAccount(
            new DeleteAccountRequest { Password = "wrong" },
            CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    public void Dispose() => _context.Dispose();
}
