using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Authentication.Login.Domain
{
    public interface ILoginTwoFactorAuthService
    {
        Task<bool> ValidateTokenAsync(TwoFactorAuthDomain.TwoFactorAuth? twoFactorAuth, string token);
        Task<bool> ValidateBackupCodeAsync(TwoFactorAuthDomain.TwoFactorAuth? twoFactorAuth, string backupCode);
    }

    public class LoginTwoFactorAuthService : ILoginTwoFactorAuthService
    {
        private readonly ILogger<LoginTwoFactorAuthService> _logger;
        private readonly TwoFactorAuthSettings _settings;

        public LoginTwoFactorAuthService(ILogger<LoginTwoFactorAuthService> logger)
        {
            _logger = logger;
            _settings = new TwoFactorAuthSettings();
        }

        public async Task<bool> ValidateTokenAsync(TwoFactorAuthDomain.TwoFactorAuth? twoFactorAuth, string token)
        {
            _logger.LogInformation("LoginTwoFactorAuthService.ValidateTokenAsync: Validating token for user {UserId}", twoFactorAuth?.UserId);
            if (twoFactorAuth is null)
            {
                _logger.LogWarning("LoginTwoFactorAuthService.ValidateTokenAsync: 2FA not found for user");
                return false;
            }

            if (!twoFactorAuth.IsEnabled)
            {
                _logger.LogWarning("LoginTwoFactorAuthService.ValidateTokenAsync: 2FA not enabled for user {UserId}", twoFactorAuth.UserId);
                return false;
            }

            if (string.IsNullOrEmpty(token) || token.Length != 6 || !token.All(char.IsDigit))
            {
                _logger.LogWarning("LoginTwoFactorAuthService.ValidateTokenAsync: Invalid token format for user {UserId}", twoFactorAuth.UserId);
                return false;
            }

            var currentTime = DateTime.UtcNow;
            var timeStep = _settings.TokenExpirySeconds;
            
            // Check current time step and adjacent time steps for clock skew tolerance
            var timeSteps = new[]
            {
                GetTimeStep(currentTime, timeStep),
                GetTimeStep(currentTime.AddSeconds(-timeStep), timeStep),
                GetTimeStep(currentTime.AddSeconds(timeStep), timeStep)
            };

            var expectedTokens = timeSteps.Select(step => GenerateTOTP(twoFactorAuth.SecretKey, step)).ToList();
            
            var isValid = expectedTokens.Contains(token);
            
            _logger.LogInformation("Token validation for user {UserId}: Token={Token}, ExpectedTokens={ExpectedTokens}, IsValid={IsValid}", 
                twoFactorAuth.UserId, token, string.Join(",", expectedTokens), isValid);
            
            if (isValid)
            {
                twoFactorAuth.LastUsedAt = currentTime;
                _logger.LogInformation("LoginTwoFactorAuthService.ValidateTokenAsync: Valid 2FA token for user {UserId}", twoFactorAuth.UserId);
            }
            else
            {
                _logger.LogWarning("LoginTwoFactorAuthService.ValidateTokenAsync: Invalid 2FA token for user {UserId}", twoFactorAuth.UserId);
            }

            return isValid;
        }

        public async Task<bool> ValidateBackupCodeAsync(TwoFactorAuthDomain.TwoFactorAuth? twoFactorAuth, string backupCode)
        {
            _logger.LogInformation("LoginTwoFactorAuthService.ValidateBackupCodeAsync: Validating backup code for user {UserId}", twoFactorAuth?.UserId);
            if (twoFactorAuth is null)
            {
                _logger.LogWarning("2FA not found for user");
                return false;
            }

            if (string.IsNullOrEmpty(backupCode))
            {
                _logger.LogWarning("LoginTwoFactorAuthService.ValidateBackupCodeAsync: Invalid backup code for user {UserId}", twoFactorAuth?.UserId);
                return false;
            }

            var backupCodeEntity = twoFactorAuth.BackupCodes
                .FirstOrDefault(bc => string.Equals(bc.Code, backupCode, StringComparison.OrdinalIgnoreCase) && !bc.IsUsed);

            if (backupCodeEntity == null)
            {
                _logger.LogWarning("LoginTwoFactorAuthService.ValidateBackupCodeAsync: Invalid or used backup code for user {UserId}", twoFactorAuth.UserId);
                return false;
            }

            backupCodeEntity.IsUsed = true;
            backupCodeEntity.UsedAt = DateTime.UtcNow;
            
            _logger.LogInformation("LoginTwoFactorAuthService.ValidateBackupCodeAsync: Valid backup code used for user {UserId}", twoFactorAuth.UserId);
            return true;
        }

        private long GetTimeStep(DateTime time, int timeStep)
        {
            var unixTime = ((DateTimeOffset)time).ToUnixTimeSeconds();
            return unixTime / timeStep;
        }

        private string GenerateTOTP(string secretKey, long timeStep)
        {
            var keyBytes = ConvertFromBase32(secretKey);
            var timeStepBytes = BitConverter.GetBytes(timeStep);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timeStepBytes);
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA1(keyBytes))
            {
                var hash = hmac.ComputeHash(timeStepBytes);
                var offset = hash[hash.Length - 1] & 0xf;
                var code = ((hash[offset] & 0x7f) << 24) |
                           ((hash[offset + 1] & 0xff) << 16) |
                           ((hash[offset + 2] & 0xff) << 8) |
                           (hash[offset + 3] & 0xff);
                return (code % 1000000).ToString("D6");
            }
        }

        private byte[] ConvertFromBase32(string input)
        {
            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new List<byte>();
            var bits = 0;
            var value = 0;

            foreach (var c in input.ToUpper())
            {
                var index = base32Chars.IndexOf(c);
                if (index == -1) continue;

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