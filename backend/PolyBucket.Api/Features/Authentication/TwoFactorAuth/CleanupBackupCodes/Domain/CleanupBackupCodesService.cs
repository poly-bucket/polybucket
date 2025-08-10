using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.CleanupBackupCodes.Repository;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.CleanupBackupCodes.Domain
{
    public interface ICleanupBackupCodesService
    {
        Task<int> CleanupExpiredBackupCodesAsync();
        Task<int> CleanupUsedBackupCodesAsync(int daysOld);
    }

    public class CleanupBackupCodesService : ICleanupBackupCodesService
    {
        private readonly ICleanupBackupCodesRepository _cleanupBackupCodesRepository;
        private readonly ILogger<CleanupBackupCodesService> _logger;

        public CleanupBackupCodesService(
            ICleanupBackupCodesRepository cleanupBackupCodesRepository,
            ILogger<CleanupBackupCodesService> logger)
        {
            _cleanupBackupCodesRepository = cleanupBackupCodesRepository;
            _logger = logger;
        }

        public async Task<int> CleanupExpiredBackupCodesAsync()
        {
            _logger.LogInformation("CleanupBackupCodesService.CleanupExpiredBackupCodesAsync: Starting cleanup of expired backup codes");
            
            var cutoffDate = DateTime.UtcNow.AddDays(-30); // Remove backup codes older than 30 days
            var deletedCount = await _cleanupBackupCodesRepository.DeleteExpiredBackupCodesAsync(cutoffDate);
            
            _logger.LogInformation("CleanupBackupCodesService.CleanupExpiredBackupCodesAsync: Deleted {Count} expired backup codes", deletedCount);
            return deletedCount;
        }

        public async Task<int> CleanupUsedBackupCodesAsync(int daysOld)
        {
            _logger.LogInformation("CleanupBackupCodesService.CleanupUsedBackupCodesAsync: Starting cleanup of used backup codes older than {DaysOld} days", daysOld);
            
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var deletedCount = await _cleanupBackupCodesRepository.DeleteUsedBackupCodesAsync(cutoffDate);
            
            _logger.LogInformation("CleanupBackupCodesService.CleanupUsedBackupCodesAsync: Deleted {Count} used backup codes", deletedCount);
            return deletedCount;
        }
    }
} 