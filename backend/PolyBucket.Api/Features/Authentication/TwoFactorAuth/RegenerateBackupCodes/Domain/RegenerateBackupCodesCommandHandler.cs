using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Repository;
using PolyBucket.Api.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Domain
{
    public class RegenerateBackupCodesCommandHandler
    {
        private readonly IRegenerateBackupCodesService _regenerateBackupCodesService;
        private readonly IRegenerateBackupCodesRepository _regenerateBackupCodesRepository;
        private readonly ILogger<RegenerateBackupCodesCommandHandler> _logger;

        public RegenerateBackupCodesCommandHandler(
            IRegenerateBackupCodesService regenerateBackupCodesService,
            IRegenerateBackupCodesRepository regenerateBackupCodesRepository,
            ILogger<RegenerateBackupCodesCommandHandler> logger)
        {
            _regenerateBackupCodesService = regenerateBackupCodesService;
            _regenerateBackupCodesRepository = regenerateBackupCodesRepository;
            _logger = logger;
        }

        public async Task<RegenerateBackupCodesResponse> Handle(RegenerateBackupCodesCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Regenerating backup codes for user {UserId}", command.UserId);

            try
            {
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

                var backupCodes = await _regenerateBackupCodesService.RegenerateBackupCodesAsync(twoFactorAuth);
                twoFactorAuth.Version++;
                
                await _regenerateBackupCodesRepository.UpdateAsync(twoFactorAuth);
                
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
                _logger.LogWarning(ex, "RegenerateBackupCodesCommandHandler.Handle: Concurrent modification detected for user {UserId}", command.UserId);
                throw new InvalidOperationException("The 2FA configuration was modified by another operation. Please try again.");
            }
        }
    }
}
