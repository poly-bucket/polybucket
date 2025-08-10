using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;
using System.Collections.Concurrent;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain
{
    public interface IEnableTwoFactorAuthService
    {
        Task<bool> EnableTwoFactorAuthAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, string token);
        Task<IEnumerable<string>> GenerateBackupCodesAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
        Task<bool> ValidateTokenAsync(TwoFactorAuthDomain.TwoFactorAuth? twoFactorAuth, string token, bool allowSetupTime = false);
    }

    public class EnableTwoFactorAuthService : IEnableTwoFactorAuthService
    {
        private readonly ILogger<EnableTwoFactorAuthService> _logger;
        private readonly TwoFactorAuthSettings _settings;
        private readonly ConcurrentDictionary<Guid, List<DateTime>> _tokenAttempts = new ConcurrentDictionary<Guid, List<DateTime>>();
        private const int MaxAttemptsPerMinute = 5;
        private const int ClockSkewToleranceSeconds = 15; // SECURITY FIX: Reduced from 30s to 15s

        public EnableTwoFactorAuthService(ILogger<EnableTwoFactorAuthService> logger)
        {
            _logger = logger;
            _settings = new TwoFactorAuthSettings();
        }

        public async Task<bool> EnableTwoFactorAuthAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, string token)
        {
            _logger.LogInformation("EnableTwoFactorAuthService.EnableTwoFactorAuthAsync: Enabling 2FA for user {UserId}", twoFactorAuth.UserId);
            var isValid = await ValidateTokenAsync(twoFactorAuth, token, allowSetupTime: true);
            
            if (isValid)
            {
                twoFactorAuth.IsEnabled = true;
                twoFactorAuth.EnabledAt = DateTime.UtcNow;
                twoFactorAuth.UpdatedAt = DateTime.UtcNow;
                
                _logger.LogInformation("EnableTwoFactorAuthService.EnableTwoFactorAuthAsync: 2FA enabled for user {UserId}", twoFactorAuth.UserId);
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<string>> GenerateBackupCodesAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth)
        {
            _logger.LogInformation("EnableTwoFactorAuthService.GenerateBackupCodesAsync: Generating backup codes for user {UserId}", twoFactorAuth.UserId);
            var backupCodes = new List<string>();
            
            for (int i = 0; i < _settings.BackupCodeCount; i++)
            {
                var code = GenerateBackupCode();
                
                // SECURITY FIX: Ensure backup code uniqueness
                while (backupCodes.Contains(code))
                {
                    code = GenerateBackupCode();
                }
                
                backupCodes.Add(code);
                
                var backupCodeEntity = new TwoFactorAuthDomain.BackupCode
                {
                    Id = Guid.NewGuid(),
                    TwoFactorAuthId = twoFactorAuth.Id,
                    Code = code,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = twoFactorAuth.UserId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = twoFactorAuth.UserId
                };
                
                twoFactorAuth.BackupCodes.Add(backupCodeEntity);
            }
            
            _logger.LogInformation("EnableTwoFactorAuthService.GenerateBackupCodesAsync: Generated {Count} backup codes for user {UserId}", backupCodes.Count, twoFactorAuth.UserId);
            return backupCodes;
        }

        public async Task<bool> ValidateTokenAsync(TwoFactorAuthDomain.TwoFactorAuth? twoFactorAuth, string token, bool allowSetupTime = false)
        {
            _logger.LogInformation("EnableTwoFactorAuthService.ValidateTokenAsync: Validating token for user {UserId}", twoFactorAuth?.UserId);
            if (twoFactorAuth is null)
            {
                _logger.LogWarning("EnableTwoFactorAuthService.ValidateTokenAsync: 2FA not found for user");
                return false;
            }

            // SECURITY FIX: Add rate limiting for token validation
            if (!CheckRateLimit(twoFactorAuth.UserId))
            {
                _logger.LogWarning("EnableTwoFactorAuthService.ValidateTokenAsync: Rate limit exceeded for user {UserId}", twoFactorAuth.UserId);
                return false;
            }

            if (!twoFactorAuth.IsEnabled && !allowSetupTime)
            {
                _logger.LogWarning("EnableTwoFactorAuthService.ValidateTokenAsync: 2FA not enabled for user {UserId}", twoFactorAuth.UserId);
                return false;
            }

            if (string.IsNullOrEmpty(token) || token.Length != 6 || !token.All(char.IsDigit))
            {
                _logger.LogWarning("EnableTwoFactorAuthService.ValidateTokenAsync: Invalid token format for user {UserId}", twoFactorAuth.UserId);
                return false;
            }

            var currentTime = DateTime.UtcNow;
            var timeStep = _settings.TokenExpirySeconds;
            
            // SECURITY FIX: Reduced clock skew tolerance from 30s to 15s
            var timeSteps = new[]
            {
                GetTimeStep(currentTime, timeStep),
                GetTimeStep(currentTime.AddSeconds(-ClockSkewToleranceSeconds), timeStep),
                GetTimeStep(currentTime.AddSeconds(ClockSkewToleranceSeconds), timeStep)
            };

            var expectedTokens = timeSteps.Select(step => GenerateTOTP(twoFactorAuth.SecretKey, step)).ToList();
            
            var isValid = expectedTokens.Contains(token);
            
            _logger.LogInformation("Token validation for user {UserId}: Token={Token}, ExpectedTokens={ExpectedTokens}, IsValid={IsValid}", 
                twoFactorAuth.UserId, token, string.Join(",", expectedTokens), isValid);
            
            if (isValid)
            {
                twoFactorAuth.LastUsedAt = currentTime;
                _logger.LogInformation("EnableTwoFactorAuthService.ValidateTokenAsync: Valid 2FA token for user {UserId}", twoFactorAuth.UserId);
            }
            else
            {
                _logger.LogWarning("EnableTwoFactorAuthService.ValidateTokenAsync: Invalid 2FA token for user {UserId}", twoFactorAuth.UserId);
            }

            return isValid;
        }

        private bool CheckRateLimit(Guid userId)
        {
            var now = DateTime.UtcNow;
            var attempts = _tokenAttempts.GetOrAdd(userId, _ => new List<DateTime>());
            
            // Remove attempts older than 1 minute
            attempts.RemoveAll(attempt => attempt < now.AddMinutes(-1));
            
            // Check if user has exceeded rate limit
            if (attempts.Count >= MaxAttemptsPerMinute)
            {
                return false;
            }
            
            // Add current attempt
            attempts.Add(now);
            return true;
        }

        private string GenerateBackupCode()
        {
            var randomBytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var code = BitConverter.ToUInt32(randomBytes, 0) % 100000000;
            return code.ToString("D8");
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