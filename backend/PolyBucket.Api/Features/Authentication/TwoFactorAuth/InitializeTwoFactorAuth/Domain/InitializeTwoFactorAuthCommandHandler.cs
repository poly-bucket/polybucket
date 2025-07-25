using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.Users.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Domain
{
    public class InitializeTwoFactorAuthCommandHandler
    {
        private readonly ITwoFactorAuthService _twoFactorAuthService;
        private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<InitializeTwoFactorAuthCommandHandler> _logger;

        public InitializeTwoFactorAuthCommandHandler(
            ITwoFactorAuthService twoFactorAuthService,
            ITwoFactorAuthRepository twoFactorAuthRepository,
            IUserRepository userRepository,
            ILogger<InitializeTwoFactorAuthCommandHandler> logger)
        {
            _twoFactorAuthService = twoFactorAuthService;
            _twoFactorAuthRepository = twoFactorAuthRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<InitializeTwoFactorAuthResponse> Handle(InitializeTwoFactorAuthCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing 2FA for user {UserId}", command.UserId);

            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // Check if 2FA is already initialized
            var existingTwoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(command.UserId);
            if (existingTwoFactorAuth is not null)
            {
                throw new InvalidOperationException("2FA is already initialized for this user");
            }

            // Initialize 2FA
            var twoFactorAuth = await _twoFactorAuthService.InitializeTwoFactorAuthAsync(user);
            
            // Save the TwoFactorAuth entity
            await _twoFactorAuthRepository.CreateAsync(twoFactorAuth);

            // Generate QR code
            var qrCodeUrl = await _twoFactorAuthService.GenerateQrCodeAsync(twoFactorAuth, user.Email);

            _logger.LogInformation("2FA initialized successfully for user {UserId}", command.UserId);

            return new InitializeTwoFactorAuthResponse
            {
                QrCodeUrl = qrCodeUrl,
                SecretKey = twoFactorAuth.SecretKey
            };
        }
    }
} 