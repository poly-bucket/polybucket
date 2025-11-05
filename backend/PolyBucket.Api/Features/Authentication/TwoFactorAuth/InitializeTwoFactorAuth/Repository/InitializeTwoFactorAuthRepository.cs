using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Repository
{
    public interface IInitializeTwoFactorAuthRepository
    {
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId);
        Task<TwoFactorAuthDomain.TwoFactorAuth> CreateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
        Task<bool> DeleteAsync(Guid twoFactorAuthId);
    }

    public class InitializeTwoFactorAuthRepository : IInitializeTwoFactorAuthRepository
    {
        private readonly PolyBucketDbContext _context;
        private readonly ILogger<InitializeTwoFactorAuthRepository> _logger;

        public InitializeTwoFactorAuthRepository(PolyBucketDbContext context, ILogger<InitializeTwoFactorAuthRepository> logger)
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

        public async Task<TwoFactorAuthDomain.TwoFactorAuth> CreateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth)
        {
            _logger.LogInformation("InitializeTwoFactorAuthRepository.CreateAsync: Creating 2FA for user {UserId}", twoFactorAuth.UserId);
            await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
            await _context.SaveChangesAsync();
            _logger.LogInformation("InitializeTwoFactorAuthRepository.CreateAsync: Successfully created 2FA for user {UserId}", twoFactorAuth.UserId);
            return twoFactorAuth;
        }

        public async Task<bool> DeleteAsync(Guid twoFactorAuthId)
        {
            var twoFactorAuth = await _context.TwoFactorAuths.FindAsync(twoFactorAuthId);
            if (twoFactorAuth == null)
            {
                return false;
            }

            _context.TwoFactorAuths.Remove(twoFactorAuth);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 