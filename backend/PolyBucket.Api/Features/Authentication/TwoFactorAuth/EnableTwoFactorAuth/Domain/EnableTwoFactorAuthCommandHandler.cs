using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Domain
{
    public class EnableTwoFactorAuthCommandHandler
    {
        private readonly ITwoFactorAuthService _twoFactorAuthService;
        private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
        private readonly ILogger<EnableTwoFactorAuthCommandHandler> _logger;

        public EnableTwoFactorAuthCommandHandler(
            ITwoFactorAuthService twoFactorAuthService,
            ITwoFactorAuthRepository twoFactorAuthRepository,
            ILogger<EnableTwoFactorAuthCommandHandler> logger)
        {
            _twoFactorAuthService = twoFactorAuthService;
            _twoFactorAuthRepository = twoFactorAuthRepository;
            _logger = logger;
        }

        public async Task<EnableTwoFactorAuthResponse> Handle(EnableTwoFactorAuthCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Enabling 2FA for user {UserId}", command.UserId);

            var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(command.UserId);
            if (twoFactorAuth is null)
            {
                throw new InvalidOperationException("2FA not initialized for this user");
            }

            if (twoFactorAuth.IsEnabled)
            {
                throw new InvalidOperationException("2FA is already enabled for this user");
            }

            var isValid = await _twoFactorAuthService.EnableTwoFactorAuthAsync(twoFactorAuth, command.Token);
            
            if (isValid)
            {
                // Generate backup codes after successful validation
                var backupCodes = await _twoFactorAuthService.GenerateBackupCodesAsync(twoFactorAuth);
                
                // Save backup codes individually to avoid concurrency issues
                foreach (var backupCode in twoFactorAuth.BackupCodes)
                {
                    await _twoFactorAuthRepository.CreateBackupCodeAsync(backupCode);
                }

                await _twoFactorAuthRepository.UpdateAsync(twoFactorAuth);
                _logger.LogInformation("2FA enabled successfully for user {UserId}", command.UserId);
                
                return new EnableTwoFactorAuthResponse
                {
                    Success = true,
                    Message = "Two-factor authentication has been enabled successfully",
                    BackupCodes = backupCodes
                };
            }
            else
            {
                _logger.LogWarning("Invalid token provided for 2FA enablement for user {UserId}", command.UserId);
                
                return new EnableTwoFactorAuthResponse
                {
                    Success = false,
                    Message = "Invalid token provided"
                };
            }
        }
    }
} 