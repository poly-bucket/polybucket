using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Repository
{
    public class TwoFactorAuthRepository : ITwoFactorAuthRepository
    {
        private readonly PolyBucketDbContext _context;

        public TwoFactorAuthRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId)
        {
            return await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .Include(tfa => tfa.User)
                .FirstOrDefaultAsync(tfa => tfa.UserId == userId);
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth> CreateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth)
        {
            await _context.TwoFactorAuths.AddAsync(twoFactorAuth);
            await _context.SaveChangesAsync();
            return twoFactorAuth;
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth)
        {
            _context.TwoFactorAuths.Update(twoFactorAuth);
            await _context.SaveChangesAsync();
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

        public async Task<TwoFactorAuthDomain.BackupCode?> GetBackupCodeAsync(Guid twoFactorAuthId, string code)
        {
            return await _context.BackupCodes
                .FirstOrDefaultAsync(bc => bc.TwoFactorAuthId == twoFactorAuthId && bc.Code == code);
        }

        public async Task<TwoFactorAuthDomain.BackupCode> CreateBackupCodeAsync(TwoFactorAuthDomain.BackupCode backupCode)
        {
            await _context.BackupCodes.AddAsync(backupCode);
            await _context.SaveChangesAsync();
            return backupCode;
        }

        public async Task<TwoFactorAuthDomain.BackupCode> UpdateBackupCodeAsync(TwoFactorAuthDomain.BackupCode backupCode)
        {
            _context.BackupCodes.Update(backupCode);
            await _context.SaveChangesAsync();
            return backupCode;
        }

        public async Task<IEnumerable<TwoFactorAuthDomain.BackupCode>> GetBackupCodesAsync(Guid twoFactorAuthId)
        {
            return await _context.BackupCodes
                .Where(bc => bc.TwoFactorAuthId == twoFactorAuthId)
                .ToListAsync();
        }

        public async Task<bool> DeleteBackupCodesAsync(Guid twoFactorAuthId)
        {
            var backupCodes = await _context.BackupCodes
                .Where(bc => bc.TwoFactorAuthId == twoFactorAuthId)
                .ToListAsync();

            if (!backupCodes.Any())
            {
                return false;
            }

            _context.BackupCodes.RemoveRange(backupCodes);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 