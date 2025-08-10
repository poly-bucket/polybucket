using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Domain;
using PolyBucket.Api.Features.ACL.Domain;
using Shouldly;
using Xunit;
using Moq;

namespace PolyBucket.Tests.Features.Authentication.Services;

public class TwoFactorAuthServiceTests
{
    private readonly ILogger<InitializeTwoFactorAuthService> _logger;
    private readonly InitializeTwoFactorAuthService _service;

    public TwoFactorAuthServiceTests()
    {
        _logger = new LoggerFactory().CreateLogger<InitializeTwoFactorAuthService>();
        _service = new InitializeTwoFactorAuthService(_logger);
    }

    [Fact]
    public async Task InitializeTwoFactorAuthAsync_ShouldCreateValidTwoFactorAuth()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            Role = new Role { Name = "User" }
        };

        // Act
        var result = await _service.InitializeTwoFactorAuthAsync(user);

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(user.Id);
        result.SecretKey.ShouldNotBeNullOrEmpty();
        result.SecretKey.ShouldMatch("^[A-Z2-7]+$"); // Base32 format validation
        result.IsEnabled.ShouldBeFalse();
        result.CreatedAt.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-1));
        result.UpdatedAt!.Value.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task GenerateQrCodeAsync_ShouldReturnValidUrl()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser"
        };

        var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SecretKey = "JBSWY3DPEHPK3PXP",
            IsEnabled = false
        };

        // Act
        var result = await _service.GenerateQrCodeAsync(twoFactorAuth, user.Email);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldStartWith("otpauth://totp/");
        result.ShouldContain("secret=JBSWY3DPEHPK3PXP");
        result.ShouldContain("issuer=PolyBucket");
        result.ShouldContain("algorithm=SHA1");
        result.ShouldContain("digits=6");
        result.ShouldContain("period=30");
    }

    [Fact]
    public async Task InitializeTwoFactorAuthAsync_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            await _service.InitializeTwoFactorAuthAsync(null!);
        });
    }

    [Fact]
    public async Task GenerateQrCodeAsync_WithNullTwoFactorAuth_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            await _service.GenerateQrCodeAsync(null!, "test@example.com");
        });
    }

    [Fact]
    public async Task GenerateQrCodeAsync_WithEmptyEmail_ShouldStillGenerateValidUrl()
    {
        // Arrange
        var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SecretKey = "JBSWY3DPEHPK3PXP",
            IsEnabled = false
        };

        // Act
        var result = await _service.GenerateQrCodeAsync(twoFactorAuth, "");

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldStartWith("otpauth://totp/");
        result.ShouldContain("secret=JBSWY3DPEHPK3PXP");
    }
} 