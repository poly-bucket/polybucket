using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Repository
{
    public interface IRegenerateBackupCodesRepository
    {
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId);
        Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
    }

    public class RegenerateBackupCodesRepository : IRegenerateBackupCodesRepository
    {
        private readonly PolyBucketDbContext _context;
        private readonly ILogger<RegenerateBackupCodesRepository> _logger;

        public RegenerateBackupCodesRepository(PolyBucketDbContext context, ILogger<RegenerateBackupCodesRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId)
        {
            return await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .Include(tfa => tfa.User)
                .AsTracking()
                .FirstOrDefaultAsync(tfa => tfa.UserId == userId);
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth)
        {
            _logger.LogInformation("RegenerateBackupCodesRepository.UpdateAsync: Updating 2FA for user {UserId}", twoFactorAuth.UserId);
            
            // Entity should already be tracked from GetByUserIdAsync
            // Backup codes are already cleared and regenerated in the service
            await _context.SaveChangesAsync();
            _logger.LogInformation("RegenerateBackupCodesRepository.UpdateAsync: Successfully updated 2FA for user {UserId}", twoFactorAuth.UserId);
            return twoFactorAuth;
        }
    }
} 