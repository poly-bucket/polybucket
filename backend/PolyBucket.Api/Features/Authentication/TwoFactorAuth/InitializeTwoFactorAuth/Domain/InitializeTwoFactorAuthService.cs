using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Domain
{
    public interface IInitializeTwoFactorAuthService
    {
        Task<TwoFactorAuthDomain.TwoFactorAuth> InitializeTwoFactorAuthAsync(User user);
        Task<string> GenerateQrCodeAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, string email);
    }

    public class InitializeTwoFactorAuthService : IInitializeTwoFactorAuthService
    {
        private readonly ILogger<InitializeTwoFactorAuthService> _logger;
        private readonly TwoFactorAuthSettings _settings;

        public InitializeTwoFactorAuthService(ILogger<InitializeTwoFactorAuthService> logger)
        {
            _logger = logger;
            _settings = new TwoFactorAuthSettings();
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth> InitializeTwoFactorAuthAsync(User user)
        {
            _logger.LogInformation("InitializeTwoFactorAuthService.InitializeTwoFactorAuthAsync: Initializing 2FA for user {UserId}", user.Id);
            var secretKey = GenerateSecretKey();
            
            // SECURITY FIX: Removed secret key logging to prevent exposure in logs
            _logger.LogInformation("InitializeTwoFactorAuthService.InitializeTwoFactorAuthAsync: Generated secret key for user {UserId}", user.Id);
            
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

            _logger.LogInformation("InitializeTwoFactorAuthService.InitializeTwoFactorAuthAsync: Initialized 2FA for user {UserId}", user.Id);
            return twoFactorAuth;
        }

        public async Task<string> GenerateQrCodeAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, string email)
        {
            _logger.LogInformation("InitializeTwoFactorAuthService.GenerateQrCodeAsync: Generating QR code for user {UserId}", twoFactorAuth.UserId);
            var issuer = HttpUtility.UrlEncode(_settings.IssuerName);
            var account = HttpUtility.UrlEncode(email);
            var secret = HttpUtility.UrlEncode(twoFactorAuth.SecretKey);
            
            var qrCodeUrl = $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period={_settings.TokenExpirySeconds}";
            
            _logger.LogInformation("InitializeTwoFactorAuthService.GenerateQrCodeAsync: Generated QR code URL for user {UserId}", twoFactorAuth.UserId);
            return qrCodeUrl;
        }

        private string GenerateSecretKey()
        {
            _logger.LogInformation("InitializeTwoFactorAuthService.GenerateSecretKey: Generating secret key");
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
            var value = 0;

            foreach (var b in input)
            {
                value = (value << 8) | b;
                bits += 8;

                while (bits >= 5)
                {
                    result.Append(base32Chars[(value >> (bits - 5)) & 31]);
                    bits -= 5;
                }
            }

            if (bits > 0)
            {
                result.Append(base32Chars[(value << (5 - bits)) & 31]);
            }

            return result.ToString();
        }
    }
} 