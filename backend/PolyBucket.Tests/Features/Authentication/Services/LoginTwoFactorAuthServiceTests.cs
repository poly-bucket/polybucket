using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        [Fact(DisplayName = "When validating a token with a null two-factor auth, the login two-factor auth service returns false.")]
        public async Task ValidateTokenAsync_WithNullTwoFactorAuth_ShouldReturnFalse()
        {
            // Arrange
            PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth? twoFactorAuth = null;

            // Act
            var result = await _service.ValidateTokenAsync(twoFactorAuth, "123456");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact(DisplayName = "When validating a token while two-factor auth is disabled, the login two-factor auth service returns false.")]
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

        [Fact(DisplayName = "When validating a null token, the login two-factor auth service returns false.")]
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

        [Fact(DisplayName = "When validating an empty token, the login two-factor auth service returns false.")]
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

        [Fact(DisplayName = "When validating a token with an invalid length, the login two-factor auth service returns false.")]
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

        [Fact(DisplayName = "When validating a token that contains non-numeric characters, the login two-factor auth service returns false.")]
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

        [Fact(DisplayName = "When validating a valid TOTP token, the login two-factor auth service returns true.")]
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

        [Fact(DisplayName = "When validating a backup code with a null two-factor auth, the login two-factor auth service returns false.")]
        public async Task ValidateBackupCodeAsync_WithNullTwoFactorAuth_ShouldReturnFalse()
        {
            // Arrange
            PolyBucket.Api.Features.Authentication.Domain.TwoFactorAuth? twoFactorAuth = null;

            // Act
            var result = await _service.ValidateBackupCodeAsync(twoFactorAuth, "12345678");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact(DisplayName = "When validating a null backup code, the login two-factor auth service returns false.")]
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

        [Fact(DisplayName = "When validating an empty backup code, the login two-factor auth service returns false.")]
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

        [Fact(DisplayName = "When validating a backup code that does not exist, the login two-factor auth service returns false.")]
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

        [Fact(DisplayName = "When validating a backup code that has already been used, the login two-factor auth service returns false.")]
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

        [Fact(DisplayName = "When validating a valid unused backup code, the login two-factor auth service returns true.")]
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

        [Fact(DisplayName = "When validating a backup code that differs only in casing, the login two-factor auth service returns true.")]
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

        [Fact(DisplayName = "When validating against multiple backup codes, the login two-factor auth service finds the matching code.")]
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
            var timeStepSeconds = 30;
            var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeStep = unixTime / timeStepSeconds;
            var keyBytes = ConvertFromBase32(secretKey);
            var timeStepBytes = BitConverter.GetBytes(timeStep);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timeStepBytes);
            }

            using var hmac = new HMACSHA1(keyBytes);
            var hash = hmac.ComputeHash(timeStepBytes);
            var offset = hash[hash.Length - 1] & 0xf;
            var code = ((hash[offset] & 0x7f) << 24) |
                       ((hash[offset + 1] & 0xff) << 16) |
                       ((hash[offset + 2] & 0xff) << 8) |
                       (hash[offset + 3] & 0xff);

            return (code % 1000000).ToString("D6");
        }

        private byte[] ConvertFromBase32(string input)
        {
            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new List<byte>();
            var bits = 0;
            var value = 0;

            foreach (var c in input.ToUpperInvariant())
            {
                var index = base32Chars.IndexOf(c);
                if (index == -1)
                {
                    continue;
                }

                value = (value << 5) | index;
                bits += 5;

                while (bits >= 8)
                {
                    result.Add((byte)(value >> (bits - 8)));
                    bits -= 8;
                }
            }

            return result.ToArray();
        }
    }
} 