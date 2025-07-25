using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Repository
{
    public interface ITwoFactorAuthRepository
    {
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId);
        Task<TwoFactorAuthDomain.TwoFactorAuth> CreateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
        Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
        Task<bool> DeleteAsync(Guid twoFactorAuthId);
        Task<TwoFactorAuthDomain.BackupCode?> GetBackupCodeAsync(Guid twoFactorAuthId, string code);
        Task<TwoFactorAuthDomain.BackupCode> CreateBackupCodeAsync(TwoFactorAuthDomain.BackupCode backupCode);
        Task<TwoFactorAuthDomain.BackupCode> UpdateBackupCodeAsync(TwoFactorAuthDomain.BackupCode backupCode);
        Task<IEnumerable<TwoFactorAuthDomain.BackupCode>> GetBackupCodesAsync(Guid twoFactorAuthId);
        Task<bool> DeleteBackupCodesAsync(Guid twoFactorAuthId);
    }
} 