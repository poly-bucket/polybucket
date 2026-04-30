using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Domain
{
    public interface IRegenerateBackupCodesService
    {
        Task<IEnumerable<string>> RegenerateBackupCodesAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, bool existingBackupCodesAlreadyRemovedFromDatabase = false);
    }

    public class RegenerateBackupCodesService : IRegenerateBackupCodesService
    {
        private readonly ILogger<RegenerateBackupCodesService> _logger;
        private readonly PolyBucketDbContext _dbContext;
        private readonly TwoFactorAuthSettings _settings;

        public RegenerateBackupCodesService(ILogger<RegenerateBackupCodesService> logger, PolyBucketDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _settings = new TwoFactorAuthSettings();
        }

        public async Task<IEnumerable<string>> RegenerateBackupCodesAsync(TwoFactorAuthDomain.TwoFactorAuth twoFactorAuth, bool existingBackupCodesAlreadyRemovedFromDatabase = false)
        {
            _logger.LogInformation("RegenerateBackupCodesService.RegenerateBackupCodesAsync: Regenerating backup codes for user {UserId}", twoFactorAuth.UserId);

            if (!existingBackupCodesAlreadyRemovedFromDatabase)
            {
                twoFactorAuth.BackupCodes.Clear();
            }
            
            var backupCodes = new List<string>();
            
            for (int i = 0; i < _settings.BackupCodeCount; i++)
            {
                var code = GenerateBackupCode();
                
                // SECURITY FIX: Ensure backup code uniqueness
                while (backupCodes.Contains(code))
                {
                    code = GenerateBackupCode();
                }
                
                backupCodes.Add(code);
                
                var backupCodeEntity = new TwoFactorAuthDomain.BackupCode
                {
                    Id = Guid.NewGuid(),
                    TwoFactorAuthId = twoFactorAuth.Id,
                    Code = code,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = twoFactorAuth.UserId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = twoFactorAuth.UserId,
                    Version = 1 // Set initial version for new backup codes
                };
                
                if (existingBackupCodesAlreadyRemovedFromDatabase)
                {
                    _dbContext.BackupCodes.Add(backupCodeEntity);
                }
                else
                {
                    twoFactorAuth.BackupCodes.Add(backupCodeEntity);
                }
            }
            
            // Update the main entity
            twoFactorAuth.UpdatedAt = DateTime.UtcNow;
            twoFactorAuth.UpdatedById = twoFactorAuth.UserId;
            
            _logger.LogInformation("RegenerateBackupCodesService.RegenerateBackupCodesAsync: Generated {Count} new backup codes for user {UserId}", backupCodes.Count, twoFactorAuth.UserId);
            return backupCodes;
        }

        private string GenerateBackupCode()
        {
            var randomBytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var code = BitConverter.ToUInt32(randomBytes, 0) % 100000000;
            return code.ToString("D8");
        }
    }
} 