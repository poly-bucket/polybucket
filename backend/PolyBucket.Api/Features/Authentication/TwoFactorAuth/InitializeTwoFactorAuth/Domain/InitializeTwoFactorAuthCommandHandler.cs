using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Repository;
using PolyBucket.Api.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TwoFactorAuthDomain = PolyBucket.Api.Features.Authentication.Domain;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Domain
{
    public class InitializeTwoFactorAuthCommandHandler
    {
        private readonly IInitializeTwoFactorAuthService _initializeTwoFactorAuthService;
        private readonly IInitializeTwoFactorAuthRepository _initializeTwoFactorAuthRepository;
        private readonly IInitializeTwoFactorAuthUserReadRepository _userReadRepository;
        private readonly PolyBucketDbContext _dbContext;
        private readonly ILogger<InitializeTwoFactorAuthCommandHandler> _logger;

        public InitializeTwoFactorAuthCommandHandler(
            IInitializeTwoFactorAuthService initializeTwoFactorAuthService,
            IInitializeTwoFactorAuthRepository initializeTwoFactorAuthRepository,
            IInitializeTwoFactorAuthUserReadRepository userReadRepository,
            PolyBucketDbContext dbContext,
            ILogger<InitializeTwoFactorAuthCommandHandler> logger)
        {
            _initializeTwoFactorAuthService = initializeTwoFactorAuthService;
            _initializeTwoFactorAuthRepository = initializeTwoFactorAuthRepository;
            _userReadRepository = userReadRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<InitializeTwoFactorAuthResponse> Handle(InitializeTwoFactorAuthCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing 2FA for user {UserId}", command.UserId);

            var user = await _userReadRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var existingTwoFactorAuth = await _initializeTwoFactorAuthRepository.GetByUserIdAsync(command.UserId);
            if (existingTwoFactorAuth is not null && existingTwoFactorAuth.IsEnabled)
            {
                throw new InvalidOperationException("2FA is already initialized for this user");
            }

            if (existingTwoFactorAuth is not null && !existingTwoFactorAuth.IsEnabled)
            {
                _dbContext.TwoFactorAuths.Remove(existingTwoFactorAuth);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Deleted existing unenabled 2FA for user {UserId} to start fresh", command.UserId);
            }

            var twoFactorAuth = await _initializeTwoFactorAuthService.InitializeTwoFactorAuthAsync(user);
            twoFactorAuth.Version = 1;
            
            await _initializeTwoFactorAuthRepository.CreateAsync(twoFactorAuth);
            
            var qrCodeUrl = await _initializeTwoFactorAuthService.GenerateQrCodeAsync(twoFactorAuth, user.Email);

            _logger.LogInformation("2FA initialized successfully for user {UserId} (saved to database with IsEnabled = false)", command.UserId);

            return new InitializeTwoFactorAuthResponse
            {
                QrCodeUrl = qrCodeUrl,
                SecretKey = twoFactorAuth.SecretKey
            };
        }
    }
} 