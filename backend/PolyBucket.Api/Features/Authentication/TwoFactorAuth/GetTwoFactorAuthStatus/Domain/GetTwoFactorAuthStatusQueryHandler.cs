using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Repository;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.GetTwoFactorAuthStatus.Domain
{
    public class GetTwoFactorAuthStatusQueryHandler
    {
        private readonly IGetTwoFactorAuthStatusRepository _getTwoFactorAuthStatusRepository;
        private readonly ILogger<GetTwoFactorAuthStatusQueryHandler> _logger;

        public GetTwoFactorAuthStatusQueryHandler(
            IGetTwoFactorAuthStatusRepository getTwoFactorAuthStatusRepository,
            ILogger<GetTwoFactorAuthStatusQueryHandler> logger)
        {
            _getTwoFactorAuthStatusRepository = getTwoFactorAuthStatusRepository;
            _logger = logger;
        }

        public async Task<GetTwoFactorAuthStatusResponse> Handle(GetTwoFactorAuthStatusQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting 2FA status for user {UserId}", query.UserId);

            var twoFactorAuth = await _getTwoFactorAuthStatusRepository.GetByUserIdAsync(query.UserId);
            
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