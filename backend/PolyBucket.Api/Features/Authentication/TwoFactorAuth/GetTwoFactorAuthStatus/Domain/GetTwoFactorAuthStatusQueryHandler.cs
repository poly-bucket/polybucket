using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Repository;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Domain
{
    public class GetTwoFactorAuthStatusQueryHandler
    {
        private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
        private readonly ILogger<GetTwoFactorAuthStatusQueryHandler> _logger;

        public GetTwoFactorAuthStatusQueryHandler(
            ITwoFactorAuthRepository twoFactorAuthRepository,
            ILogger<GetTwoFactorAuthStatusQueryHandler> logger)
        {
            _twoFactorAuthRepository = twoFactorAuthRepository;
            _logger = logger;
        }

        public async Task<GetTwoFactorAuthStatusResponse> Handle(GetTwoFactorAuthStatusQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting 2FA status for user {UserId}", query.UserId);

            var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(query.UserId);
            
            if (twoFactorAuth is null)
            {
                return new GetTwoFactorAuthStatusResponse
                {
                    IsEnabled = false,
                    IsInitialized = false,
                    RemainingBackupCodes = 0
                };
            }

            var remainingBackupCodes = twoFactorAuth.BackupCodes.Count(bc => !bc.IsUsed);

            return new GetTwoFactorAuthStatusResponse
            {
                IsEnabled = twoFactorAuth.IsEnabled,
                IsInitialized = true,
                EnabledAt = twoFactorAuth.EnabledAt,
                LastUsedAt = twoFactorAuth.LastUsedAt,
                RemainingBackupCodes = remainingBackupCodes
            };
        }
    }
} 