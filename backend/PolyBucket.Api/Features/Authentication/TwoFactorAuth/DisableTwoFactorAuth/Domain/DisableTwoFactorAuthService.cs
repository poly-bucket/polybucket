using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain
{
    public interface IDisableTwoFactorAuthService
    {
        Task<bool> DisableTwoFactorAuthAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
    }

    public class DisableTwoFactorAuthService : IDisableTwoFactorAuthService
    {
        private readonly ILogger<DisableTwoFactorAuthService> _logger;

        public DisableTwoFactorAuthService(ILogger<DisableTwoFactorAuthService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> DisableTwoFactorAuthAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth)
        {
            _logger.LogInformation("DisableTwoFactorAuthService.DisableTwoFactorAuthAsync: Disabling 2FA for user {UserId}", twoFactorAuth.UserId);
            twoFactorAuth.IsEnabled = false;
            twoFactorAuth.EnabledAt = null;
            twoFactorAuth.UpdatedAt = DateTime.UtcNow;
            
            // Mark all backup codes as used
            foreach (var backupCode in twoFactorAuth.BackupCodes)
            {
                backupCode.IsUsed = true;
                backupCode.UsedAt = DateTime.UtcNow;
            }
            
            _logger.LogInformation("DisableTwoFactorAuthService.DisableTwoFactorAuthAsync: 2FA disabled for user {UserId}", twoFactorAuth.UserId);
            return true;
        }
    }
} 