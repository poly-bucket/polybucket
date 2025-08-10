using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Repository;
using PolyBucket.Api.Features.Users.Repository;
using PolyBucket.Api.Data;
using Microsoft.EntityFrameworkCore;
using System;
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

            // CONCURRENCY FIX: Use database-level locking to prevent race conditions
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var twoFactorAuth = await _enableTwoFactorAuthRepository.GetByUserIdWithLockAsync(command.UserId);
                if (twoFactorAuth is null)
                {
                    _logger.LogError("EnableTwoFactorAuthCommandHandler.Handle: 2FA not initialized for user {UserId}", command.UserId);
                    throw new InvalidOperationException("2FA not initialized for this user. Please initialize 2FA first.");
                }

                if (twoFactorAuth.IsEnabled)
                {
                    _logger.LogError("EnableTwoFactorAuthCommandHandler.Handle: 2FA is already enabled for user {UserId}", command.UserId);
                    throw new InvalidOperationException("2FA is already enabled for this user");
                }

                // CONCURRENCY FIX: Store current version for optimistic concurrency control
                var currentVersion = twoFactorAuth.Version;

                // Validate the token against the existing entity (without modifying it)
                var isValid = await _enableTwoFactorAuthService.ValidateTokenAsync(twoFactorAuth, command.Token, allowSetupTime: true);
                
                if (isValid)
                {
                    // CONCURRENCY FIX: Check version before any modifications
                    var existingEntity = await _dbContext.TwoFactorAuths
                        .Include(tfa => tfa.BackupCodes)
                        .FirstOrDefaultAsync(tfa => tfa.Id == twoFactorAuth.Id);

                    if (existingEntity == null)
                    {
                        _logger.LogError("EnableTwoFactorAuthCommandHandler.Handle: TwoFactorAuth with Id {Id} not found", twoFactorAuth.Id);
                        throw new InvalidOperationException($"TwoFactorAuth with Id {twoFactorAuth.Id} not found");
                    }

                    // CONCURRENCY FIX: Check version for optimistic concurrency control
                    if (existingEntity.Version != currentVersion)
                    {
                        _logger.LogWarning("EnableTwoFactorAuthCommandHandler.Handle: Version mismatch for TwoFactorAuth {Id}. Expected: {Expected}, Actual: {Actual}", 
                            twoFactorAuth.Id, currentVersion, existingEntity.Version);
                        throw new InvalidOperationException("Concurrent modification detected. Please try again.");
                    }

                    // Now modify the entity after successful version check
                    twoFactorAuth.IsEnabled = true;
                    twoFactorAuth.EnabledAt = DateTime.UtcNow;
                    twoFactorAuth.UpdatedAt = DateTime.UtcNow;
                    twoFactorAuth.Version = currentVersion + 1;
                    
                    // Generate backup codes after successful validation
                    var backupCodes = await _enableTwoFactorAuthService.GenerateBackupCodesAsync(twoFactorAuth);
                    
                    // Update the TwoFactorAuth entity in the database
                    var updatedTwoFactorAuth = await _enableTwoFactorAuthRepository.UpdateAsync(twoFactorAuth);
                    
                    // Commit the transaction
                    await transaction.CommitAsync(cancellationToken);
                    
                    _logger.LogInformation("EnableTwoFactorAuthCommandHandler.Handle: 2FA enabled successfully for user {UserId}", command.UserId);
                    
                    return new EnableTwoFactorAuthResponse
                    {
                        Success = true,
                        Message = "Two-factor authentication has been enabled successfully",
                        BackupCodes = backupCodes
                    };
                }
                else
                {
                    // Rollback the transaction on validation failure
                    await transaction.RollbackAsync(cancellationToken);
                    
                    _logger.LogWarning("EnableTwoFactorAuthCommandHandler.Handle: Invalid token provided for 2FA enablement for user {UserId}", command.UserId);
                    
                    return new EnableTwoFactorAuthResponse
                    {
                        Success = false,
                        Message = "Invalid token provided"
                    };
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
} 