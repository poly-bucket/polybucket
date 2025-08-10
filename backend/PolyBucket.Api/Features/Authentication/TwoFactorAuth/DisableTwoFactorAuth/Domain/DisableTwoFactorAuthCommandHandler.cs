using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Repository;
using PolyBucket.Api.Features.Users.Repository;
using PolyBucket.Api.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain
{
    public class DisableTwoFactorAuthCommandHandler
    {
        private readonly IDisableTwoFactorAuthService _disableTwoFactorAuthService;
        private readonly IDisableTwoFactorAuthRepository _disableTwoFactorAuthRepository;
        private readonly IUserRepository _userRepository;
        private readonly PolyBucketDbContext _dbContext;
        private readonly ILogger<DisableTwoFactorAuthCommandHandler> _logger;

        public DisableTwoFactorAuthCommandHandler(
            IDisableTwoFactorAuthService disableTwoFactorAuthService,
            IDisableTwoFactorAuthRepository disableTwoFactorAuthRepository,
            IUserRepository userRepository,
            PolyBucketDbContext dbContext,
            ILogger<DisableTwoFactorAuthCommandHandler> logger)
        {
            _disableTwoFactorAuthService = disableTwoFactorAuthService;
            _disableTwoFactorAuthRepository = disableTwoFactorAuthRepository;
            _userRepository = userRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<DisableTwoFactorAuthResponse> Handle(DisableTwoFactorAuthCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Disabling 2FA for user {UserId}", command.UserId);

            // CONCURRENCY FIX: Use database-level locking to prevent race conditions
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var twoFactorAuth = await _disableTwoFactorAuthRepository.GetByUserIdWithLockAsync(command.UserId);
                if (twoFactorAuth is null)
                {
                    throw new InvalidOperationException("2FA not found for this user");
                }

                if (!twoFactorAuth.IsEnabled)
                {
                    throw new InvalidOperationException("2FA is not enabled for this user");
                }

                // CONCURRENCY FIX: Store current version for optimistic concurrency control
                var currentVersion = twoFactorAuth.Version;

                var success = await _disableTwoFactorAuthService.DisableTwoFactorAuthAsync(twoFactorAuth);
                
                if (success)
                {
                    // CONCURRENCY FIX: Increment version for optimistic concurrency control
                    twoFactorAuth.Version = currentVersion + 1;
                    
                    // Update with version check
                    await _disableTwoFactorAuthRepository.UpdateWithVersionAsync(twoFactorAuth, currentVersion);
                    
                    // Commit the transaction
                    await transaction.CommitAsync(cancellationToken);
                    
                    _logger.LogInformation("2FA disabled successfully for user {UserId}", command.UserId);
                    
                    return new DisableTwoFactorAuthResponse
                    {
                        Success = true,
                        Message = "Two-factor authentication has been disabled successfully"
                    };
                }
                else
                {
                    // Rollback the transaction on failure
                    await transaction.RollbackAsync(cancellationToken);
                    
                    _logger.LogWarning("Failed to disable 2FA for user {UserId}", command.UserId);
                    
                    return new DisableTwoFactorAuthResponse
                    {
                        Success = false,
                        Message = "Failed to disable two-factor authentication"
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