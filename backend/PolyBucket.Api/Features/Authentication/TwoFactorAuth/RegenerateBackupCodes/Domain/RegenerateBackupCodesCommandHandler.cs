using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Repository;
using PolyBucket.Api.Features.Users.Repository;
using PolyBucket.Api.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Domain
{
    public class RegenerateBackupCodesCommandHandler
    {
        private readonly IRegenerateBackupCodesService _regenerateBackupCodesService;
        private readonly IRegenerateBackupCodesRepository _regenerateBackupCodesRepository;
        private readonly IUserRepository _userRepository;
        private readonly PolyBucketDbContext _dbContext;
        private readonly ILogger<RegenerateBackupCodesCommandHandler> _logger;

        public RegenerateBackupCodesCommandHandler(
            IRegenerateBackupCodesService regenerateBackupCodesService,
            IRegenerateBackupCodesRepository regenerateBackupCodesRepository,
            IUserRepository userRepository,
            PolyBucketDbContext dbContext,
            ILogger<RegenerateBackupCodesCommandHandler> logger)
        {
            _regenerateBackupCodesService = regenerateBackupCodesService;
            _regenerateBackupCodesRepository = regenerateBackupCodesRepository;
            _userRepository = userRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<RegenerateBackupCodesResponse> Handle(RegenerateBackupCodesCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Regenerating backup codes for user {UserId}", command.UserId);

            // CONCURRENCY FIX: Use database-level locking to prevent race conditions
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var twoFactorAuth = await _regenerateBackupCodesRepository.GetByUserIdWithLockAsync(command.UserId);
                if (twoFactorAuth is null)
                {
                    _logger.LogError("RegenerateBackupCodesCommandHandler.Handle: 2FA not found for user {UserId}", command.UserId);
                    throw new InvalidOperationException("2FA not initialized for this user");
                }

                if (!twoFactorAuth.IsEnabled)
                {
                    _logger.LogError("RegenerateBackupCodesCommandHandler.Handle: 2FA is not enabled for user {UserId}", command.UserId);
                    throw new InvalidOperationException("2FA is not enabled for this user");
                }

                // CONCURRENCY FIX: Store current version for optimistic concurrency control
                var currentVersion = twoFactorAuth.Version;

                // Generate new backup codes
                var backupCodes = await _regenerateBackupCodesService.RegenerateBackupCodesAsync(twoFactorAuth);
                
                // CONCURRENCY FIX: Increment version for optimistic concurrency control
                twoFactorAuth.Version = currentVersion + 1;
                
                // Update the TwoFactorAuth entity in the database with version check
                var updatedTwoFactorAuth = await _regenerateBackupCodesRepository.UpdateWithVersionAsync(twoFactorAuth, currentVersion);
                
                // Commit the transaction
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogInformation("RegenerateBackupCodesCommandHandler.Handle: Backup codes regenerated successfully for user {UserId}", command.UserId);
                
                return new RegenerateBackupCodesResponse
                {
                    Success = true,
                    Message = "Backup codes have been regenerated successfully",
                    BackupCodes = backupCodes
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
} 