using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using PolyBucket.Api.Features.Authentication.Login.Repository;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Api.Common.Models;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IAuthenticationRepository> _mockAuthRepository;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<LoginCommandHandler>> _mockLogger;
    private readonly Mock<ILoginTwoFactorAuthService> _mockLoginTwoFactorAuthService;
    private readonly Mock<ILoginTwoFactorAuthRepository> _mockLoginTwoFactorAuthRepository;
    private readonly Mock<IAuthenticationSettingsService> _mockAuthenticationSettingsService;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockAuthRepository = new Mock<IAuthenticationRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<LoginCommandHandler>>();
        _mockLoginTwoFactorAuthService = new Mock<ILoginTwoFactorAuthService>();
        _mockLoginTwoFactorAuthRepository = new Mock<ILoginTwoFactorAuthRepository>();
        _mockAuthenticationSettingsService = new Mock<IAuthenticationSettingsService>();

        _handler = new LoginCommandHandler(
            _mockAuthRepository.Object,
            _mockTokenService.Object,
            _mockPasswordHasher.Object,
            _mockConfiguration.Object,
            _mockLogger.Object,
            null, // Context is not needed for this test
            _mockLoginTwoFactorAuthService.Object,
            _mockLoginTwoFactorAuthRepository.Object,
            _mockAuthenticationSettingsService.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnRefreshToken()
    {
        // Arrange
        var command = new LoginCommand
        {
            EmailOrUsername = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashedpassword",
            RequiresPasswordChange = false
        };

        var authResponse = new AuthenticationResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(60),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockAuthRepository.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _mockTokenService.Setup(x => x.GenerateAuthenticationResponse(It.IsAny<User>()))
            .Returns(authResponse);
        _mockAuthRepository.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken());
        _mockAuthRepository.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
            .Returns(Task.CompletedTask);
        _mockLoginTwoFactorAuthRepository.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Token.ShouldBe("access-token");
        result.RefreshToken.ShouldBe("refresh-token");
        result.TokenExpiresAt.ShouldBe(authResponse.AccessTokenExpiresAt);
        result.RefreshTokenExpiresAt.ShouldBe(authResponse.RefreshTokenExpiresAt);
        result.RequiresPasswordChange.ShouldBeFalse();
        result.RequiresFirstTimeSetup.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCommand
        {
            EmailOrUsername = "test@example.com",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashedpassword"
        };

        _mockAuthRepository.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);
        _mockAuthRepository.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCommand
        {
            EmailOrUsername = "nonexistent@example.com",
            Password = "password123"
        };

        _mockAuthRepository.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockAuthRepository.Setup(x => x.CreateLoginRecordAsync(It.IsAny<UserLogin>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });
    }
} 