using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Repository;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain
{
    public class DisableTwoFactorAuthCommandHandler
    {
        private readonly IDisableTwoFactorAuthService _disableTwoFactorAuthService;
        private readonly IDisableTwoFactorAuthRepository _disableTwoFactorAuthRepository;
        private readonly PolyBucketDbContext _dbContext;
        private readonly ILogger<DisableTwoFactorAuthCommandHandler> _logger;

        public DisableTwoFactorAuthCommandHandler(
            IDisableTwoFactorAuthService disableTwoFactorAuthService,
            IDisableTwoFactorAuthRepository disableTwoFactorAuthRepository,
            PolyBucketDbContext dbContext,
            ILogger<DisableTwoFactorAuthCommandHandler> logger)
        {
            _disableTwoFactorAuthService = disableTwoFactorAuthService;
            _disableTwoFactorAuthRepository = disableTwoFactorAuthRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<DisableTwoFactorAuthResponse> Handle(DisableTwoFactorAuthCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Disabling 2FA for user {UserId}", command.UserId);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
                var twoFactorAuth = await _disableTwoFactorAuthRepository.GetByUserIdAsync(command.UserId);
                if (twoFactorAuth is null)
                {
                    throw new InvalidOperationException("2FA not found for this user");
                }

                if (!twoFactorAuth.IsEnabled)
                {
                    throw new InvalidOperationException("2FA is not enabled for this user");
                }

                var success = await _disableTwoFactorAuthService.DisableTwoFactorAuthAsync(twoFactorAuth);

                if (success)
                {
                    twoFactorAuth.Version++;
                    await _disableTwoFactorAuthRepository.UpdateAsync(twoFactorAuth);

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("2FA disabled successfully for user {UserId}", command.UserId);

                    return new DisableTwoFactorAuthResponse
                    {
                        Success = true,
                        Message = "Two-factor authentication has been disabled successfully"
                    };
                }

                await transaction.RollbackAsync(cancellationToken);

                _logger.LogWarning("Failed to disable 2FA for user {UserId}", command.UserId);

                return new DisableTwoFactorAuthResponse
                {
                    Success = false,
                    Message = "Failed to disable two-factor authentication"
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "DisableTwoFactorAuthCommandHandler.Handle: Concurrent modification detected for user {UserId}", command.UserId);
                throw new InvalidOperationException("The 2FA configuration was modified by another operation. Please try again.");
            }
            catch (DbUpdateException ex) when (IsPostgresSerializationFailure(ex))
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "DisableTwoFactorAuthCommandHandler.Handle: Serialization conflict for user {UserId}", command.UserId);
                throw new InvalidOperationException("The 2FA configuration was modified by another operation. Please try again.");
            }
        }

        private static bool IsPostgresSerializationFailure(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException pg && pg.SqlState == "40001";
        }
    }
}
