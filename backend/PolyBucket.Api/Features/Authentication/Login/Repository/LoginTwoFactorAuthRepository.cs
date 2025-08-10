using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.Login.Repository
{
    public interface ILoginTwoFactorAuthRepository
    {
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId);
        Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
    }

    public class LoginTwoFactorAuthRepository : ILoginTwoFactorAuthRepository
    {
        private readonly PolyBucketDbContext _context;
        private readonly ILogger<LoginTwoFactorAuthRepository> _logger;

        public LoginTwoFactorAuthRepository(PolyBucketDbContext context, ILogger<LoginTwoFactorAuthRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId)
        {
            return await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .Include(tfa => tfa.User)
                .FirstOrDefaultAsync(tfa => tfa.UserId == userId);
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth)
        {
            _logger.LogInformation("LoginTwoFactorAuthRepository.UpdateAsync: Updating 2FA for user {UserId}", twoFactorAuth.UserId);
            
            // Get the existing entity from the database to ensure we're working with the correct state
            var existingEntity = await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .FirstOrDefaultAsync(tfa => tfa.Id == twoFactorAuth.Id);

            if (existingEntity == null)
            {
                _logger.LogError("LoginTwoFactorAuthRepository.UpdateAsync: TwoFactorAuth with Id {Id} not found", twoFactorAuth.Id);
                throw new InvalidOperationException($"TwoFactorAuth with Id {twoFactorAuth.Id} not found");
            }

            // Update the main entity properties
            existingEntity.LastUsedAt = twoFactorAuth.LastUsedAt;
            existingEntity.UpdatedAt = twoFactorAuth.UpdatedAt;
            existingEntity.UpdatedById = twoFactorAuth.UpdatedById;

            // Update backup codes that were used during login
            foreach (var backupCode in twoFactorAuth.BackupCodes)
            {
                var existingBackupCode = existingEntity.BackupCodes.FirstOrDefault(bc => bc.Id == backupCode.Id);
                if (existingBackupCode != null)
                {
                    existingBackupCode.IsUsed = backupCode.IsUsed;
                    existingBackupCode.UsedAt = backupCode.UsedAt;
                }
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("LoginTwoFactorAuthRepository.UpdateAsync: Successfully updated 2FA for user {UserId}", twoFactorAuth.UserId);
            return existingEntity;
        }
    }
} 