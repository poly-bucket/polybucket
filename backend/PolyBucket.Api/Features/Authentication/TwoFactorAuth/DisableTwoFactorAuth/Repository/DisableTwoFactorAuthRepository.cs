using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Repository
{
    public interface IDisableTwoFactorAuthRepository
    {
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId);
        Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
    }

    public class DisableTwoFactorAuthRepository : IDisableTwoFactorAuthRepository
    {
        private readonly PolyBucketDbContext _context;
        private readonly ILogger<DisableTwoFactorAuthRepository> _logger;

        public DisableTwoFactorAuthRepository(PolyBucketDbContext context, ILogger<DisableTwoFactorAuthRepository> logger)
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
            _logger.LogInformation("DisableTwoFactorAuthRepository.UpdateAsync: Updating 2FA for user {UserId}", twoFactorAuth.UserId);
            
            // Entity should already be tracked from GetByUserIdAsync
            // Backup codes are already updated in the service
            await _context.SaveChangesAsync();
            _logger.LogInformation("DisableTwoFactorAuthRepository.UpdateAsync: Successfully updated 2FA for user {UserId}", twoFactorAuth.UserId);
            return twoFactorAuth;
        }
    }
} 