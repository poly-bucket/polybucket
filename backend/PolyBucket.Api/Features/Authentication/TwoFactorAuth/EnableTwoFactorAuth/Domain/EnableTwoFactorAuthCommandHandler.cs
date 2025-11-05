using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Repository;
using PolyBucket.Api.Features.Users.Repository;
using PolyBucket.Api.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain
{
    public class EnableTwoFactorAuthCommandHandler
    {
        private readonly IEnableTwoFactorAuthService _enableTwoFactorAuthService;
        private readonly IEnableTwoFactorAuthRepository _enableTwoFactorAuthRepository;
        private readonly IUserRepository _userRepository;
        private readonly PolyBucketDbContext _dbContext;
        private readonly ILogger<EnableTwoFactorAuthCommandHandler> _logger;

        public EnableTwoFactorAuthCommandHandler(
            IEnableTwoFactorAuthService enableTwoFactorAuthService,
            IEnableTwoFactorAuthRepository enableTwoFactorAuthRepository,
            IUserRepository userRepository,
            PolyBucketDbContext dbContext,
            ILogger<EnableTwoFactorAuthCommandHandler> logger)
        {
            _enableTwoFactorAuthService = enableTwoFactorAuthService;
            _enableTwoFactorAuthRepository = enableTwoFactorAuthRepository;
            _userRepository = userRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<EnableTwoFactorAuthResponse> Handle(EnableTwoFactorAuthCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Enabling 2FA for user {UserId}", command.UserId);

            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user == null)
            {
                _logger.LogError("EnableTwoFactorAuthCommandHandler.Handle: User not found for user {UserId}", command.UserId);
                throw new ArgumentException("User not found");
            }

            try
            {
                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Starting 2FA enable for user {UserId}", command.UserId);
                
                var twoFactorAuth = await _enableTwoFactorAuthRepository.GetByUserIdAsync(command.UserId);
                if (twoFactorAuth is null)
                {
                    _logger.LogError("EnableTwoFactorAuthCommandHandler.Handle: 2FA not initialized for user {UserId}", command.UserId);
                    throw new InvalidOperationException("2FA not initialized for this user. Please initialize 2FA first.");
                }

                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Loaded 2FA entity - Id: {Id}, Version: {Version}, IsEnabled: {IsEnabled}, BackupCodesCount: {BackupCodesCount}", 
                    twoFactorAuth.Id, twoFactorAuth.Version, twoFactorAuth.IsEnabled, twoFactorAuth.BackupCodes?.Count ?? 0);

                if (twoFactorAuth.IsEnabled)
                {
                    _logger.LogError("EnableTwoFactorAuthCommandHandler.Handle: 2FA is already enabled for user {UserId}", command.UserId);
                    throw new InvalidOperationException("2FA is already enabled for this user");
                }

                // Store original version for concurrency check
                var originalVersion = twoFactorAuth.Version;
                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Original Version: {Version}", originalVersion);
                
                var entryBeforeValidation = _dbContext.Entry(twoFactorAuth);
                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Entity state before validation: {State}", entryBeforeValidation.State);
                
                var isValid = await _enableTwoFactorAuthService.ValidateTokenAsync(twoFactorAuth, command.Token, allowSetupTime: true);
                
                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Token validation result: {IsValid}", isValid);
                
                if (!isValid)
                {
                    _logger.LogWarning("EnableTwoFactorAuthCommandHandler.Handle: Invalid token provided for 2FA enablement for user {UserId}", command.UserId);
                    return new EnableTwoFactorAuthResponse
                    {
                        Success = false,
                        Message = "Invalid token provided"
                    };
                }

                // Check entity state after validation
                var entryAfterValidation = _dbContext.Entry(twoFactorAuth);
                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Entity state after validation: {State}", entryAfterValidation.State);
                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: LastUsedAt modified: {IsModified}, Value: {Value}", 
                    entryAfterValidation.Property(tfa => tfa.LastUsedAt).IsModified, twoFactorAuth.LastUsedAt);

                // Clear LastUsedAt change if ValidateTokenAsync modified it during setup
                // We don't want to save LastUsedAt during initial enable, only during actual use
                if (!twoFactorAuth.IsEnabled)
                {
                    if (entryAfterValidation.Property(tfa => tfa.LastUsedAt).IsModified)
                    {
                        _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Clearing LastUsedAt modification for user {UserId}", command.UserId);
                        entryAfterValidation.Property(tfa => tfa.LastUsedAt).IsModified = false;
                        twoFactorAuth.LastUsedAt = null;
                    }
                }

                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Setting IsEnabled=true, Version from {OldVersion} to {NewVersion}", 
                    originalVersion, originalVersion + 1);

                twoFactorAuth.IsEnabled = true;
                twoFactorAuth.EnabledAt = DateTime.UtcNow;
                twoFactorAuth.UpdatedAt = DateTime.UtcNow;
                twoFactorAuth.Version = originalVersion + 1;
                
                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Before GenerateBackupCodes - Version: {Version}, BackupCodesCount: {Count}", 
                    twoFactorAuth.Version, twoFactorAuth.BackupCodes?.Count ?? 0);
                
                var backupCodes = await _enableTwoFactorAuthService.GenerateBackupCodesAsync(twoFactorAuth);
                
                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: After GenerateBackupCodes - Version: {Version}, BackupCodesCount: {Count}, GeneratedCodesCount: {GeneratedCount}", 
                    twoFactorAuth.Version, twoFactorAuth.BackupCodes?.Count ?? 0, backupCodes.Count());
                
                // Log all tracked entities before save
                var trackedEntities = _dbContext.ChangeTracker.Entries()
                    .Where(e => e.Entity is TwoFactorAuthDomain.TwoFactorAuth || e.Entity is TwoFactorAuthDomain.BackupCode)
                    .ToList();
                
                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Tracked entities before SaveChanges - Count: {Count}", trackedEntities.Count);
                foreach (var tracked in trackedEntities)
                {
                    if (tracked.Entity is TwoFactorAuthDomain.TwoFactorAuth tfa)
                    {
                        _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Tracked TwoFactorAuth - Id: {Id}, State: {State}, Version: {Version}, IsEnabled: {IsEnabled}", 
                            tfa.Id, tracked.State, tfa.Version, tfa.IsEnabled);
                    }
                    else if (tracked.Entity is TwoFactorAuthDomain.BackupCode bc)
                    {
                        _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: Tracked BackupCode - Id: {Id}, State: {State}, Version: {Version}, Code: {Code}", 
                            bc.Id, tracked.State, bc.Version, bc.Code);
                    }
                }
                
                await _enableTwoFactorAuthRepository.UpdateAsync(twoFactorAuth);
                
                _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: 2FA enabled successfully for user {UserId}", command.UserId);
                
                return new EnableTwoFactorAuthResponse
                {
                    Success = true,
                    Message = "Two-factor authentication has been enabled successfully",
                    BackupCodes = backupCodes
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "EnableTwoFactorAuthCommandHandler.Handle: Concurrent modification detected for user {UserId}", command.UserId);
                throw new InvalidOperationException("The 2FA configuration was modified by another operation. Please try again.");
            }
        }
    }
} 