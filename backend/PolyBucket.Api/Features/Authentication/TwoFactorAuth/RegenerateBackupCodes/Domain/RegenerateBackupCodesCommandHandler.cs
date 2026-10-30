using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Repository;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Domain
{
    public class RegenerateBackupCodesCommandHandler
    {
        private readonly IRegenerateBackupCodesService _regenerateBackupCodesService;
        private readonly IRegenerateBackupCodesRepository _regenerateBackupCodesRepository;
        private readonly PolyBucketDbContext _dbContext;
        private readonly ILogger<RegenerateBackupCodesCommandHandler> _logger;

        public RegenerateBackupCodesCommandHandler(
            IRegenerateBackupCodesService regenerateBackupCodesService,
            IRegenerateBackupCodesRepository regenerateBackupCodesRepository,
            PolyBucketDbContext dbContext,
            ILogger<RegenerateBackupCodesCommandHandler> logger)
        {
            _regenerateBackupCodesService = regenerateBackupCodesService;
            _regenerateBackupCodesRepository = regenerateBackupCodesRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<RegenerateBackupCodesResponse> Handle(RegenerateBackupCodesCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Regenerating backup codes for user {UserId}", command.UserId);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            try
            {
                await _regenerateBackupCodesRepository.LockByUserIdAsync(command.UserId, cancellationToken);

                var twoFactorAuth = await _regenerateBackupCodesRepository.GetByUserIdAsync(command.UserId);
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

                await _regenerateBackupCodesRepository.DeleteBackupCodesForTwoFactorAuthAsync(twoFactorAuth.Id, cancellationToken);

                var backupCodes = await _regenerateBackupCodesService.RegenerateBackupCodesAsync(twoFactorAuth, existingBackupCodesAlreadyRemovedFromDatabase: true);
                twoFactorAuth.Version++;
                await _regenerateBackupCodesRepository.SaveChangesAsync();

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("RegenerateBackupCodesCommandHandler.Handle: Backup codes regenerated successfully for user {UserId}", command.UserId);

                return new RegenerateBackupCodesResponse
                {
                    Success = true,
                    Message = "Backup codes have been regenerated successfully",
                    BackupCodes = backupCodes
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await RollbackQuietlyAsync(transaction, command.UserId, cancellationToken);
                _logger.LogWarning(ex, "RegenerateBackupCodesCommandHandler.Handle: Concurrent modification detected for user {UserId}", command.UserId);
                throw new InvalidOperationException("The 2FA configuration was modified by another operation. Please try again.");
            }
            catch (Exception ex) when (IsPostgresSerializationOrDeadlock(ex))
            {
                await RollbackQuietlyAsync(transaction, command.UserId, cancellationToken);
                _logger.LogWarning(ex, "RegenerateBackupCodesCommandHandler.Handle: Serialization conflict for user {UserId}", command.UserId);
                throw new InvalidOperationException("The 2FA configuration was modified by another operation. Please try again.");
            }
        }

        private static bool IsPostgresSerializationOrDeadlock(Exception ex)
        {
            for (var current = ex; current != null; current = current.InnerException)
            {
                if (current is PostgresException pg && (pg.SqlState == "40001" || pg.SqlState == "40P01"))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task RollbackQuietlyAsync(IDbContextTransaction transaction, Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "RegenerateBackupCodesCommandHandler.Handle: Transaction rollback failed for user {UserId}", userId);
            }
        }
    }
}
