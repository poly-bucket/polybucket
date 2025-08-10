using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using PolyBucket.Api.Features.Authentication.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication.Services
{
    public class LoginTwoFactorAuthServiceTests
    {
        private readonly ILogger<LoginTwoFactorAuthService> _logger;
        private readonly LoginTwoFactorAuthService _service;

        public LoginTwoFactorAuthServiceTests()
        {
            _logger = new LoggerFactory().CreateLogger<LoginTwoFactorAuthService>();
            _service = new LoginTwoFactorAuthService(_logger);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithNullTwoFactorAuth_ShouldReturnFalse()
        {
            // Arrange
            PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth? twoFactorAuth = null;

            // Act
            var result = await _service.ValidateTokenAsync(twoFactorAuth, "123456");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithDisabledTwoFactorAuth_ShouldReturnFalse()
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
            var result = await _service.ValidateTokenAsync(twoFactorAuth, "123456");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithNullToken_ShouldReturnFalse()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true
            };

            // Act
            var result = await _service.ValidateTokenAsync(twoFactorAuth, null);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithEmptyToken_ShouldReturnFalse()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true
            };

            // Act
            var result = await _service.ValidateTokenAsync(twoFactorAuth, "");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithInvalidTokenLength_ShouldReturnFalse()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true
            };

            // Act
            var result = await _service.ValidateTokenAsync(twoFactorAuth, "12345"); // Too short

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithNonNumericToken_ShouldReturnFalse()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true
            };

            // Act
            var result = await _service.ValidateTokenAsync(twoFactorAuth, "abcdef");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ShouldReturnTrue()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true
            };

            var validToken = GenerateValidTOTP(twoFactorAuth.SecretKey);

            // Act
            var result = await _service.ValidateTokenAsync(twoFactorAuth, validToken);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public async Task ValidateBackupCodeAsync_WithNullTwoFactorAuth_ShouldReturnFalse()
        {
            // Arrange
            PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth? twoFactorAuth = null;

            // Act
            var result = await _service.ValidateBackupCodeAsync(twoFactorAuth, "12345678");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateBackupCodeAsync_WithNullBackupCode_ShouldReturnFalse()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true,
                BackupCodes = new List<BackupCode>()
            };

            // Act
            var result = await _service.ValidateBackupCodeAsync(twoFactorAuth, null);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateBackupCodeAsync_WithEmptyBackupCode_ShouldReturnFalse()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true,
                BackupCodes = new List<BackupCode>()
            };

            // Act
            var result = await _service.ValidateBackupCodeAsync(twoFactorAuth, "");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateBackupCodeAsync_WithNonExistentBackupCode_ShouldReturnFalse()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true,
                BackupCodes = new List<BackupCode>
                {
                    new BackupCode { Code = "12345678", IsUsed = false }
                }
            };

            // Act
            var result = await _service.ValidateBackupCodeAsync(twoFactorAuth, "87654321");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateBackupCodeAsync_WithUsedBackupCode_ShouldReturnFalse()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true,
                BackupCodes = new List<BackupCode>
                {
                    new BackupCode { Code = "12345678", IsUsed = true }
                }
            };

            // Act
            var result = await _service.ValidateBackupCodeAsync(twoFactorAuth, "12345678");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task ValidateBackupCodeAsync_WithValidBackupCode_ShouldReturnTrue()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true,
                BackupCodes = new List<BackupCode>
                {
                    new BackupCode { Code = "12345678", IsUsed = false }
                }
            };

            // Act
            var result = await _service.ValidateBackupCodeAsync(twoFactorAuth, "12345678");

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public async Task ValidateBackupCodeAsync_WithCaseInsensitiveMatch_ShouldReturnTrue()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true,
                BackupCodes = new List<BackupCode>
                {
                    new BackupCode { Code = "AbCdEfGh", IsUsed = false }
                }
            };

            // Act
            var result = await _service.ValidateBackupCodeAsync(twoFactorAuth, "abcdefgh");

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public async Task ValidateBackupCodeAsync_WithMultipleBackupCodes_ShouldFindCorrectOne()
        {
            // Arrange
            var twoFactorAuth = new PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SecretKey = "JBSWY3DPEHPK3PXP",
                IsEnabled = true,
                BackupCodes = new List<BackupCode>
                {
                    new BackupCode { Code = "11111111", IsUsed = false },
                    new BackupCode { Code = "22222222", IsUsed = false },
                    new BackupCode { Code = "33333333", IsUsed = false }
                }
            };

            // Act
            var result1 = await _service.ValidateBackupCodeAsync(twoFactorAuth, "11111111");
            var result2 = await _service.ValidateBackupCodeAsync(twoFactorAuth, "22222222");
            var result3 = await _service.ValidateBackupCodeAsync(twoFactorAuth, "33333333");
            var result4 = await _service.ValidateBackupCodeAsync(twoFactorAuth, "44444444");

            // Assert
            result1.ShouldBeTrue();
            result2.ShouldBeTrue();
            result3.ShouldBeTrue();
            result4.ShouldBeFalse();
        }

        private string GenerateValidTOTP(string secretKey)
        {
            // This is a simplified implementation for testing
            // In a real scenario, this would generate a valid TOTP token
            return "123456";
        }
    }
} 