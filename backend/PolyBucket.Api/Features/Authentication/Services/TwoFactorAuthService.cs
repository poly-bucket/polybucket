using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly ILogger<TwoFactorAuthService> _logger;
        private readonly TwoFactorAuthSettings _settings;

        public TwoFactorAuthService(ILogger<TwoFactorAuthService> logger)
        {
            _logger = logger;
            _settings = new TwoFactorAuthSettings();
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth> InitializeTwoFactorAuthAsync(User user)
        {
            var secretKey = GenerateSecretKey();
            
            _logger.LogInformation("Generated secret key for user {UserId}: {SecretKey}", user.Id, secretKey);
            
            var twoFactorAuth = new TwoFactorAuthDomain.TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SecretKey = secretKey,
                IsEnabled = false,
                CreatedAt = DateTime.UtcNow,
                CreatedById = user.Id,
                UpdatedAt = DateTime.UtcNow,
                UpdatedById = user.Id
            };

            _logger.LogInformation("Initialized 2FA for user {UserId}", user.Id);
            return twoFactorAuth;
        }

        public async Task<string> GenerateQrCodeAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, string email)
        {
            var issuer = HttpUtility.UrlEncode(_settings.IssuerName);
            var account = HttpUtility.UrlEncode(email);
            var secret = HttpUtility.UrlEncode(twoFactorAuth.SecretKey);
            
            var qrCodeUrl = $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period={_settings.TokenExpirySeconds}";
            
            _logger.LogInformation("Generated QR code URL for user {UserId}: {QrCodeUrl}", twoFactorAuth.UserId, qrCodeUrl);
            return qrCodeUrl;
        }

        public async Task<bool> ValidateTokenAsync(TwoFactorAuthDomain.TwoFactorAuth? twoFactorAuth, string token, bool allowSetupTime = false)
        {
            if (twoFactorAuth is null)
            {
                _logger.LogWarning("2FA not found for user");
                return false;
            }

            if (!twoFactorAuth.IsEnabled && !allowSetupTime)
            {
                _logger.LogWarning("2FA not enabled for user {UserId}", twoFactorAuth.UserId);
                return false;
            }

            if (string.IsNullOrEmpty(token) || token.Length != 6 || !token.All(char.IsDigit))
            {
                _logger.LogWarning("Invalid token format for user {UserId}", twoFactorAuth.UserId);
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
                _logger.LogInformation("Valid 2FA token for user {UserId}", twoFactorAuth.UserId);
            }
            else
            {
                _logger.LogWarning("Invalid 2FA token for user {UserId}", twoFactorAuth.UserId);
            }

            return isValid;
        }

        public async Task<bool> ValidateBackupCodeAsync(TwoFactorAuthDomain.TwoFactorAuth? twoFactorAuth, string backupCode)
        {
            if (twoFactorAuth is null)
            {
                _logger.LogWarning("2FA not found for user");
                return false;
            }

            if (string.IsNullOrEmpty(backupCode))
            {
                return false;
            }

            var backupCodeEntity = twoFactorAuth.BackupCodes
                .FirstOrDefault(bc => bc.Code == backupCode && !bc.IsUsed);

            if (backupCodeEntity == null)
            {
                _logger.LogWarning("Invalid or used backup code for user {UserId}", twoFactorAuth.UserId);
                return false;
            }

            backupCodeEntity.IsUsed = true;
            backupCodeEntity.UsedAt = DateTime.UtcNow;
            
            _logger.LogInformation("Valid backup code used for user {UserId}", twoFactorAuth.UserId);
            return true;
        }

        public async Task<IEnumerable<string>> GenerateBackupCodesAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth)
        {
            var backupCodes = new List<string>();
            var existingCodes = twoFactorAuth.BackupCodes.Select(bc => bc.Code).ToHashSet();

            for (int i = 0; i < _settings.BackupCodeCount; i++)
            {
                string code;
                do
                {
                    code = GenerateBackupCode(_settings.BackupCodeLength);
                } while (existingCodes.Contains(code));

                backupCodes.Add(code);
                existingCodes.Add(code);

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

            _logger.LogInformation("Generated {Count} backup codes for user {UserId}", backupCodes.Count, twoFactorAuth.UserId);
            return backupCodes;
        }

        public async Task<bool> EnableTwoFactorAuthAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, string token)
        {
            var isValid = await ValidateTokenAsync(twoFactorAuth, token, allowSetupTime: true);
            
            if (isValid)
            {
                twoFactorAuth.IsEnabled = true;
                twoFactorAuth.EnabledAt = DateTime.UtcNow;
                twoFactorAuth.UpdatedAt = DateTime.UtcNow;
                
                _logger.LogInformation("2FA enabled for user {UserId}", twoFactorAuth.UserId);
                return true;
            }

            return false;
        }

        public async Task<bool> DisableTwoFactorAuthAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth)
        {
            twoFactorAuth.IsEnabled = false;
            twoFactorAuth.EnabledAt = null;
            twoFactorAuth.UpdatedAt = DateTime.UtcNow;
            
            // Mark all backup codes as used
            foreach (var backupCode in twoFactorAuth.BackupCodes)
            {
                backupCode.IsUsed = true;
                backupCode.UsedAt = DateTime.UtcNow;
            }
            
            _logger.LogInformation("2FA disabled for user {UserId}", twoFactorAuth.UserId);
            return true;
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth?> GetTwoFactorAuthAsync(Guid userId)
        {
            // This will be implemented by the repository
            throw new NotImplementedException("This method should be implemented by the repository layer");
        }

        public async Task<bool> IsTwoFactorEnabledAsync(Guid userId)
        {
            var twoFactorAuth = await GetTwoFactorAuthAsync(userId);
            return twoFactorAuth?.IsEnabled ?? false;
        }

        public async Task<bool> IsTwoFactorRequiredAsync(User user)
        {
            if (_settings.RequireTwoFactorForAdmins && user.Role?.Name == "Admin")
            {
                return true;
            }

            return false;
        }

        private string GenerateSecretKey()
        {
            var randomBytes = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return ConvertToBase32(randomBytes);
        }

        private string ConvertToBase32(byte[] input)
        {
            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new StringBuilder();
            var bits = 0;
            var bitCount = 0;

            foreach (var b in input)
            {
                bits = (bits << 8) | b;
                bitCount += 8;

                while (bitCount >= 5)
                {
                    bitCount -= 5;
                    result.Append(base32Chars[(bits >> bitCount) & 31]);
                }
            }

            if (bitCount > 0)
            {
                bits <<= (5 - bitCount);
                result.Append(base32Chars[bits & 31]);
            }

            return result.ToString();
        }

        private long GetTimeStep(DateTime time, int timeStepSeconds)
        {
            var unixTime = ((DateTimeOffset)time).ToUnixTimeSeconds();
            return unixTime / timeStepSeconds;
        }

        private string GenerateTOTP(string secretKey, long timeStep)
        {
            var key = ConvertFromBase32(secretKey);
            var timeStepBytes = BitConverter.GetBytes(timeStep);
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timeStepBytes);
            }

            using (var hmac = new HMACSHA1(key))
            {
                var hash = hmac.ComputeHash(timeStepBytes);
                var offset = hash[^1] & 0xf;
                
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
            input = input.ToUpper().Replace(" ", "");
            var result = new List<byte>();
            var bits = 0;
            var bitCount = 0;

            foreach (var c in input)
            {
                var value = base32Chars.IndexOf(c);
                if (value == -1) continue; // Skip invalid characters

                bits = (bits << 5) | value;
                bitCount += 5;

                while (bitCount >= 8)
                {
                    bitCount -= 8;
                    result.Add((byte)((bits >> bitCount) & 0xFF));
                }
            }

            return result.ToArray();
        }

        private string GenerateBackupCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
} 