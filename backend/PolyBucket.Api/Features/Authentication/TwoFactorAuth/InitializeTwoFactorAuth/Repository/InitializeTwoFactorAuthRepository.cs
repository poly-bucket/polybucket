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
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdWithLockAsync(Guid userId);
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
                .FirstOrDefaultAsync(tfa => tfa.UserId == userId);
        }

        // CONCURRENCY FIX: Add database-level locking to prevent race conditions
        public async Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdWithLockAsync(Guid userId)
        {
            // Use raw SQL with FOR UPDATE to implement proper row-level locking
            var result = await _context.TwoFactorAuths
                .FromSqlRaw("SELECT * FROM \"TwoFactorAuths\" WHERE \"UserId\" = {0} FOR UPDATE", userId)
                .FirstOrDefaultAsync();
            
            if (result != null)
            {
                // Load related entities separately
                await _context.Entry(result).Reference(tfa => tfa.User).LoadAsync();
                await _context.Entry(result).Collection(tfa => tfa.BackupCodes).LoadAsync();
            }
            
            return result;
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