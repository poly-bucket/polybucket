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
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdWithLockAsync(Guid userId);
        Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
        Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateWithVersionAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, int expectedVersion);
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

        public async Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth)
        {
            _logger.LogInformation("RegenerateBackupCodesRepository.UpdateAsync: Updating 2FA for user {UserId}", twoFactorAuth.UserId);
            
            // Get the existing entity from the database to ensure we're working with the correct state
            var existingEntity = await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .FirstOrDefaultAsync(tfa => tfa.Id == twoFactorAuth.Id);

            if (existingEntity == null)
            {
                _logger.LogError("RegenerateBackupCodesRepository.UpdateAsync: TwoFactorAuth with Id {Id} not found", twoFactorAuth.Id);
                throw new InvalidOperationException($"TwoFactorAuth with Id {twoFactorAuth.Id} not found");
            }

            // Update the main entity properties
            existingEntity.UpdatedAt = twoFactorAuth.UpdatedAt;
            existingEntity.UpdatedById = twoFactorAuth.UpdatedById;

            // Clear existing backup codes and add new ones
            existingEntity.BackupCodes.Clear();
            foreach (var backupCode in twoFactorAuth.BackupCodes)
            {
                existingEntity.BackupCodes.Add(backupCode);
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("RegenerateBackupCodesRepository.UpdateAsync: Successfully updated 2FA for user {UserId}", twoFactorAuth.UserId);
            return existingEntity;
        }

        // CONCURRENCY FIX: Add optimistic concurrency control with version checking
        public async Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateWithVersionAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, int expectedVersion)
        {
            _logger.LogInformation("RegenerateBackupCodesRepository.UpdateWithVersionAsync: Updating 2FA for user {UserId} with version check", twoFactorAuth.UserId);
            
            // Get the existing entity from the database to ensure we're working with the correct state
            var existingEntity = await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .FirstOrDefaultAsync(tfa => tfa.Id == twoFactorAuth.Id);

            if (existingEntity == null)
            {
                _logger.LogError("RegenerateBackupCodesRepository.UpdateWithVersionAsync: TwoFactorAuth with Id {Id} not found", twoFactorAuth.Id);
                throw new InvalidOperationException($"TwoFactorAuth with Id {twoFactorAuth.Id} not found");
            }

            // CONCURRENCY FIX: Check version for optimistic concurrency control
            if (existingEntity.Version != expectedVersion)
            {
                _logger.LogWarning("RegenerateBackupCodesRepository.UpdateWithVersionAsync: Version mismatch for TwoFactorAuth {Id}. Expected: {Expected}, Actual: {Actual}", 
                    twoFactorAuth.Id, expectedVersion, existingEntity.Version);
                throw new InvalidOperationException("Concurrent modification detected. Please try again.");
            }

            // Update the main entity properties
            existingEntity.UpdatedAt = twoFactorAuth.UpdatedAt;
            existingEntity.UpdatedById = twoFactorAuth.UpdatedById;
            existingEntity.Version = twoFactorAuth.Version; // Update version

            // Clear existing backup codes and add new ones
            existingEntity.BackupCodes.Clear();
            foreach (var backupCode in twoFactorAuth.BackupCodes)
            {
                existingEntity.BackupCodes.Add(backupCode);
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("RegenerateBackupCodesRepository.UpdateWithVersionAsync: Successfully updated 2FA for user {UserId}", twoFactorAuth.UserId);
            return existingEntity;
        }
    }
} 