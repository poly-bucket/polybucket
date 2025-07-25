using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.DisableTwoFactorAuth.Domain
{
    public class DisableTwoFactorAuthCommandHandler
    {
        private readonly ITwoFactorAuthService _twoFactorAuthService;
        private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
        private readonly ILogger<DisableTwoFactorAuthCommandHandler> _logger;

        public DisableTwoFactorAuthCommandHandler(
            ITwoFactorAuthService twoFactorAuthService,
            ITwoFactorAuthRepository twoFactorAuthRepository,
            ILogger<DisableTwoFactorAuthCommandHandler> logger)
        {
            _twoFactorAuthService = twoFactorAuthService;
            _twoFactorAuthRepository = twoFactorAuthRepository;
            _logger = logger;
        }

        public async Task<DisableTwoFactorAuthResponse> Handle(DisableTwoFactorAuthCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Disabling 2FA for user {UserId}", command.UserId);

            var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(command.UserId);
            if (twoFactorAuth is null)
            {
                throw new InvalidOperationException("2FA not found for this user");
            }

            if (!twoFactorAuth.IsEnabled)
            {
                throw new InvalidOperationException("2FA is not enabled for this user");
            }

            var success = await _twoFactorAuthService.DisableTwoFactorAuthAsync(twoFactorAuth);
            
            if (success)
            {
                await _twoFactorAuthRepository.UpdateAsync(twoFactorAuth);
                _logger.LogInformation("2FA disabled successfully for user {UserId}", command.UserId);
                
                return new DisableTwoFactorAuthResponse
                {
                    Success = true,
                    Message = "Two-factor authentication has been disabled successfully"
                };
            }
            else
            {
                _logger.LogWarning("Failed to disable 2FA for user {UserId}", command.UserId);
                
                return new DisableTwoFactorAuthResponse
                {
                    Success = false,
                    Message = "Failed to disable two-factor authentication"
                };
            }
        }
    }
} 