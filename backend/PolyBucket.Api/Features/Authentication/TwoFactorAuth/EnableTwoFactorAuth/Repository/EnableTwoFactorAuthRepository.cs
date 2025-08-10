using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Repository
{
    public interface IEnableTwoFactorAuthRepository
    {
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId);
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdWithLockAsync(Guid userId);
        Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
        Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateWithVersionAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, int expectedVersion);
    }

    public class EnableTwoFactorAuthRepository : IEnableTwoFactorAuthRepository
    {
        private readonly PolyBucketDbContext _context;
        private readonly ILogger<EnableTwoFactorAuthRepository> _logger;

        public EnableTwoFactorAuthRepository(PolyBucketDbContext context, ILogger<EnableTwoFactorAuthRepository> logger)
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
            _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: Updating 2FA for user {UserId}", twoFactorAuth.UserId);
            
            // Get the existing entity from the database to ensure we're working with the correct state
            var existingEntity = await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .FirstOrDefaultAsync(tfa => tfa.Id == twoFactorAuth.Id);

            if (existingEntity == null)
            {
                _logger.LogError("EnableTwoFactorAuthRepository.UpdateAsync: TwoFactorAuth with Id {Id} not found", twoFactorAuth.Id);
                throw new InvalidOperationException($"TwoFactorAuth with Id {twoFactorAuth.Id} not found");
            }

            // Update the main entity properties
            existingEntity.IsEnabled = twoFactorAuth.IsEnabled;
            existingEntity.EnabledAt = twoFactorAuth.EnabledAt;
            existingEntity.LastUsedAt = twoFactorAuth.LastUsedAt;
            existingEntity.UpdatedAt = twoFactorAuth.UpdatedAt;
            existingEntity.UpdatedById = twoFactorAuth.UpdatedById;

            // Add new backup codes to the database
            foreach (var backupCode in twoFactorAuth.BackupCodes)
            {
                // Check if this backup code already exists in the database
                var existingBackupCode = existingEntity.BackupCodes.FirstOrDefault(bc => bc.Id == backupCode.Id);
                if (existingBackupCode == null)
                {
                    // This is a new backup code that needs to be added
                    _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: Adding new backup code {BackupCodeId} for user {UserId}", backupCode.Id, twoFactorAuth.UserId);
                    existingEntity.BackupCodes.Add(backupCode);
                    await _context.BackupCodes.AddAsync(backupCode);
                }
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: Successfully updated 2FA for user {UserId}", twoFactorAuth.UserId);
            return existingEntity;
        }

        // CONCURRENCY FIX: Add optimistic concurrency control with version checking
        public async Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateWithVersionAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, int expectedVersion)
        {
            _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateWithVersionAsync: Updating 2FA for user {UserId} with version check", twoFactorAuth.UserId);
            
            // Get the existing entity from the database to ensure we're working with the correct state
            var existingEntity = await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .FirstOrDefaultAsync(tfa => tfa.Id == twoFactorAuth.Id);

            if (existingEntity == null)
            {
                _logger.LogError("EnableTwoFactorAuthRepository.UpdateWithVersionAsync: TwoFactorAuth with Id {Id} not found", twoFactorAuth.Id);
                throw new InvalidOperationException($"TwoFactorAuth with Id {twoFactorAuth.Id} not found");
            }

            // CONCURRENCY FIX: Check version for optimistic concurrency control
            if (existingEntity.Version != expectedVersion)
            {
                _logger.LogWarning("EnableTwoFactorAuthRepository.UpdateWithVersionAsync: Version mismatch for TwoFactorAuth {Id}. Expected: {Expected}, Actual: {Actual}", 
                    twoFactorAuth.Id, expectedVersion, existingEntity.Version);
                throw new InvalidOperationException("Concurrent modification detected. Please try again.");
            }

            // Update the main entity properties
            existingEntity.IsEnabled = twoFactorAuth.IsEnabled;
            existingEntity.EnabledAt = twoFactorAuth.EnabledAt;
            existingEntity.LastUsedAt = twoFactorAuth.LastUsedAt;
            existingEntity.UpdatedAt = twoFactorAuth.UpdatedAt;
            existingEntity.UpdatedById = twoFactorAuth.UpdatedById;
            existingEntity.Version = twoFactorAuth.Version; // Update version

            // Add new backup codes to the database
            foreach (var backupCode in twoFactorAuth.BackupCodes)
            {
                // Check if this backup code already exists in the database
                var existingBackupCode = existingEntity.BackupCodes.FirstOrDefault(bc => bc.Id == backupCode.Id);
                if (existingBackupCode == null)
                {
                    // This is a new backup code that needs to be added
                    _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateWithVersionAsync: Adding new backup code {BackupCodeId} for user {UserId}", backupCode.Id, twoFactorAuth.UserId);
                    existingEntity.BackupCodes.Add(backupCode);
                }
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateWithVersionAsync: Successfully updated 2FA for user {UserId}", twoFactorAuth.UserId);
            return existingEntity;
        }
    }
} 