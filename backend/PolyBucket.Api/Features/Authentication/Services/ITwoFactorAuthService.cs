using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public interface ITwoFactorAuthService
    {
        Task<TwoFactorAuthDomain.TwoFactorAuth> InitializeTwoFactorAuthAsync(User user);
        Task<string> GenerateQrCodeAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, string email);
        Task<bool> ValidateTokenAsync(TwoFactorAuthDomain.TwoFactorAuth? twoFactorAuth, string token, bool allowSetupTime = false);
        Task<bool> ValidateBackupCodeAsync(TwoFactorAuthDomain.TwoFactorAuth? twoFactorAuth, string backupCode);
        Task<IEnumerable<string>> GenerateBackupCodesAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
        Task<bool> EnableTwoFactorAuthAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, string token);
        Task<bool> DisableTwoFactorAuthAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetTwoFactorAuthAsync(Guid userId);
        Task<bool> IsTwoFactorEnabledAsync(Guid userId);
        Task<bool> IsTwoFactorRequiredAsync(User user);
    }
} 