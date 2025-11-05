using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Repository
{
    public interface IEnableTwoFactorAuthRepository
    {
        Task<TwoFactorAuthDomain.TwoFactorAuth?> GetByUserIdAsync(Guid userId);
        Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth);
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
            var entity = await _context.TwoFactorAuths
                .Include(tfa => tfa.BackupCodes)
                .Include(tfa => tfa.User)
                .AsTracking()
                .FirstOrDefaultAsync(tfa => tfa.UserId == userId);
            
            if (entity != null)
            {
                _logger.LogInformation("EnableTwoFactorAuthRepository.GetByUserIdAsync: Loaded 2FA - Id: {Id}, Version: {Version}, BackupCodesCount: {Count}", 
                    entity.Id, entity.Version, entity.BackupCodes?.Count ?? 0);
                
                // Log existing BackupCodes if any
                if (entity.BackupCodes != null && entity.BackupCodes.Any())
                {
                    foreach (var bc in entity.BackupCodes)
                    {
                        _logger.LogInformation("EnableTwoFactorAuthRepository.GetByUserIdAsync: Existing BackupCode - Id: {Id}, Version: {Version}, IsUsed: {IsUsed}", 
                            bc.Id, bc.Version, bc.IsUsed);
                    }
                }
            }
            
            return entity;
        }

        public async Task<TwoFactorAuthDomain.TwoFactorAuth> UpdateAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth)
        {
            _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: Updating 2FA for user {UserId}", twoFactorAuth.UserId);

            // Log entity state before save
            var entry = _context.Entry(twoFactorAuth);
            _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: TwoFactorAuth entity state: {State}, Version: {Version}", 
                entry.State, twoFactorAuth.Version);
            
            // Log all modified properties
            var modifiedProperties = entry.Properties
                .Where(p => p.IsModified)
                .Select(p => $"{p.Metadata.Name}={p.CurrentValue}")
                .ToList();
            _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: Modified properties: {Properties}", string.Join(", ", modifiedProperties));

            // Log BackupCodes state BEFORE fixing
            var backupCodeEntriesBefore = _context.ChangeTracker.Entries<TwoFactorAuthDomain.BackupCode>()
                .Where(e => e.Entity.TwoFactorAuthId == twoFactorAuth.Id)
                .ToList();
            _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: BackupCodes tracked BEFORE fix - Count: {Count}", backupCodeEntriesBefore.Count);
            foreach (var bcEntry in backupCodeEntriesBefore)
            {
                _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: BackupCode BEFORE - Id: {Id}, State: {State}, Version: {Version}, Code: {Code}", 
                    bcEntry.Entity.Id, bcEntry.State, bcEntry.Entity.Version, bcEntry.Entity.Code);
            }

            // CRITICAL FIX: Ensure new BackupCodes are marked as Added, not Modified
            // New BackupCodes should have state = Added, existing ones should be Unchanged
            var existingBackupCodeIds = twoFactorAuth.BackupCodes
                .Where(bc => bc.Id != Guid.Empty)
                .Select(bc => bc.Id)
                .ToHashSet();
            
            // Get all BackupCodes for this TwoFactorAuth from the database to check which are new
            var existingBackupCodesInDb = await _context.BackupCodes
                .Where(bc => bc.TwoFactorAuthId == twoFactorAuth.Id)
                .Select(bc => bc.Id)
                .ToListAsync();
            
            _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: Existing BackupCodes in DB - Count: {Count}, Ids: {Ids}", 
                existingBackupCodesInDb.Count, string.Join(", ", existingBackupCodesInDb));
            
            // Ensure all new BackupCodes are explicitly marked as Added
            foreach (var backupCode in twoFactorAuth.BackupCodes)
            {
                var bcEntry = _context.Entry(backupCode);
                
                // If this BackupCode doesn't exist in the database, it must be new
                if (!existingBackupCodesInDb.Contains(backupCode.Id))
                {
                    if (bcEntry.State != EntityState.Added)
                    {
                        _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: Marking BackupCode {Id} as Added (was {State})", 
                            backupCode.Id, bcEntry.State);
                        bcEntry.State = EntityState.Added;
                    }
                }
                else
                {
                    // If it exists in DB but we're trying to modify it, that's wrong - we should only be adding new ones
                    if (bcEntry.State == EntityState.Modified)
                    {
                        _logger.LogWarning("EnableTwoFactorAuthRepository.UpdateAsync: BackupCode {Id} exists in DB but is marked as Modified. This shouldn't happen during enable.", 
                            backupCode.Id);
                        // Reset to Unchanged since we're not modifying existing codes
                        bcEntry.State = EntityState.Unchanged;
                    }
                }
            }

            // Log BackupCodes state AFTER fixing
            var backupCodeEntriesAfter = _context.ChangeTracker.Entries<TwoFactorAuthDomain.BackupCode>()
                .Where(e => e.Entity.TwoFactorAuthId == twoFactorAuth.Id)
                .ToList();
            _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: BackupCodes tracked AFTER fix - Count: {Count}", backupCodeEntriesAfter.Count);
            foreach (var bcEntry in backupCodeEntriesAfter)
            {
                _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: BackupCode AFTER - Id: {Id}, State: {State}, Version: {Version}", 
                    bcEntry.Entity.Id, bcEntry.State, bcEntry.Entity.Version);
            }

            // Entity should already be tracked from GetByUserIdAsync
            // Backup codes are already added to the entity in the service
            // EF Core will automatically detect changes and save them
            await _context.SaveChangesAsync();
            _logger.LogInformation("EnableTwoFactorAuthRepository.UpdateAsync: Successfully updated 2FA for user {UserId}", twoFactorAuth.UserId);
            return twoFactorAuth;
        }
    }
} 