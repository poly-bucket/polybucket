using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Repository
{
    public interface IRegenerateBackupCodesRepository
    {
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId);
        Task DeleteBackupCodesForTwoFactorAuthAsync(Guid twoFactorAuthId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync();
    }

    public class RegenerateBackupCodesRepository : IRegenerateBackupCodesRepository
    {
        private readonly PolyBucketDbContext _context;

        public RegenerateBackupCodesRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId)
        {
            return await _context.TwoFactorAuths
                .Include(tfa => tfa.User)
                .AsTracking()
                .FirstOrDefaultAsync(tfa => tfa.UserId == userId);
        }

        public async Task DeleteBackupCodesForTwoFactorAuthAsync(Guid twoFactorAuthId, CancellationToken cancellationToken = default)
        {
            await _context.BackupCodes
                .Where(bc => bc.TwoFactorAuthId == twoFactorAuthId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}