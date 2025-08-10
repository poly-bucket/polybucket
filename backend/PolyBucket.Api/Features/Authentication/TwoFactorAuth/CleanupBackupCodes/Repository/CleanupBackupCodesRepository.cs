using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.CleanupBackupCodes.Repository
{
    public interface ICleanupBackupCodesRepository
    {
        Task<int> DeleteExpiredBackupCodesAsync(DateTime cutoffDate);
        Task<int> DeleteUsedBackupCodesAsync(DateTime cutoffDate);
    }

    public class CleanupBackupCodesRepository : ICleanupBackupCodesRepository
    {
        private readonly PolyBucketDbContext _context;
        private readonly ILogger<CleanupBackupCodesRepository> _logger;

        public CleanupBackupCodesRepository(PolyBucketDbContext context, ILogger<CleanupBackupCodesRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> DeleteExpiredBackupCodesAsync(DateTime cutoffDate)
        {
            _logger.LogInformation("CleanupBackupCodesRepository.DeleteExpiredBackupCodesAsync: Deleting backup codes older than {CutoffDate}", cutoffDate);
            
            var expiredBackupCodes = await _context.BackupCodes
                .Where(bc => bc.CreatedAt < cutoffDate)
                .ToListAsync();

            if (expiredBackupCodes.Any())
            {
                _context.BackupCodes.RemoveRange(expiredBackupCodes);
                await _context.SaveChangesAsync();
                _logger.LogInformation("CleanupBackupCodesRepository.DeleteExpiredBackupCodesAsync: Deleted {Count} expired backup codes", expiredBackupCodes.Count);
            }

            return expiredBackupCodes.Count;
        }

        public async Task<int> DeleteUsedBackupCodesAsync(DateTime cutoffDate)
        {
            _logger.LogInformation("CleanupBackupCodesRepository.DeleteUsedBackupCodesAsync: Deleting used backup codes older than {CutoffDate}", cutoffDate);
            
            var usedBackupCodes = await _context.BackupCodes
                .Where(bc => bc.IsUsed && bc.UsedAt < cutoffDate)
                .ToListAsync();

            if (usedBackupCodes.Any())
            {
                _context.BackupCodes.RemoveRange(usedBackupCodes);
                await _context.SaveChangesAsync();
                _logger.LogInformation("CleanupBackupCodesRepository.DeleteUsedBackupCodesAsync: Deleted {Count} used backup codes", usedBackupCodes.Count);
            }

            return usedBackupCodes.Count;
        }
    }
} 