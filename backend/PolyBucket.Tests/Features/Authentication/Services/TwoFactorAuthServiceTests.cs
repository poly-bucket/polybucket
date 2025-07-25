using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.ACL.Domain;
using Shouldly;
using Xunit;
using Moq;

namespace PolyBucket.Tests.Features.Authentication.Services;

public class TwoFactorAuthServiceTests
{
    private readonly ILogger<TwoFactorAuthService> _logger;
    private readonly TwoFactorAuthService _service;

    public TwoFactorAuthServiceTests()
    {
        _logger = new Mock<ILogger<TwoFactorAuthService>>().Object;
        _service = new TwoFactorAuthService(_logger);
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

        var twoFactorAuth = await _service.InitializeTwoFactorAuthAsync(user);

        // Act
        var qrCodeUrl = await _service.GenerateQrCodeAsync(twoFactorAuth, user.Email);

        // Assert
        qrCodeUrl.ShouldNotBeNullOrEmpty();
        qrCodeUrl.ShouldStartWith("otpauth://totp/");
        qrCodeUrl.ShouldContain("secret=");
        qrCodeUrl.ShouldContain("issuer=");
        
        // Extract and validate the secret key from the QR code URL
        var secretMatch = System.Text.RegularExpressions.Regex.Match(qrCodeUrl, @"secret=([A-Z2-7]+)");
        secretMatch.Success.ShouldBeTrue();
        secretMatch.Groups[1].Value.ShouldMatch("^[A-Z2-7]+$"); // Base32 format validation
    }

    [Fact]
    public async Task GenerateBackupCodesAsync_ShouldReturnCorrectNumberOfCodes()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser"
        };

        var twoFactorAuth = await _service.InitializeTwoFactorAuthAsync(user);

        // Act
        var backupCodes = await _service.GenerateBackupCodesAsync(twoFactorAuth);

        // Assert
        backupCodes.ShouldNotBeNull();
        backupCodes.Count().ShouldBe(10); // Default backup code count
        backupCodes.All(code => !string.IsNullOrEmpty(code) && code.Length == 8).ShouldBeTrue();
        backupCodes.Distinct().Count().ShouldBe(10); // All codes should be unique
    }

    [Fact]
    public async Task SecretKey_ShouldBeValidBase32Format()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser"
        };

        // Act
        var twoFactorAuth = await _service.InitializeTwoFactorAuthAsync(user);

        // Assert
        twoFactorAuth.SecretKey.ShouldNotBeNullOrEmpty();
        twoFactorAuth.SecretKey.ShouldMatch("^[A-Z2-7]+$"); // Base32 format validation
        twoFactorAuth.SecretKey.Length.ShouldBeGreaterThan(20); // Base32 encoded 20 bytes should be ~32 characters
    }

    [Fact]
    public async Task ValidateTokenAsync_WithDisabledTwoFactorAuth_ShouldReturnFalse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser"
        };

        var twoFactorAuth = await _service.InitializeTwoFactorAuthAsync(user);
        twoFactorAuth.IsEnabled = false;

        // Act
        var result = await _service.ValidateTokenAsync(twoFactorAuth, "123456");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidTokenFormat_ShouldReturnFalse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser"
        };

        var twoFactorAuth = await _service.InitializeTwoFactorAuthAsync(user);
        twoFactorAuth.IsEnabled = true;

        // Act
        var result = await _service.ValidateTokenAsync(twoFactorAuth, "invalid");

        // Assert
        result.ShouldBeFalse();
    }
} 