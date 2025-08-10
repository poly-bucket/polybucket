using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Repository;
using PolyBucket.Api.Features.Users.Repository;
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
        private readonly IUserRepository _userRepository;
        private readonly PolyBucketDbContext _dbContext;
        private readonly ILogger<InitializeTwoFactorAuthCommandHandler> _logger;

        public InitializeTwoFactorAuthCommandHandler(
            IInitializeTwoFactorAuthService initializeTwoFactorAuthService,
            IInitializeTwoFactorAuthRepository initializeTwoFactorAuthRepository,
            IUserRepository userRepository,
            PolyBucketDbContext dbContext,
            ILogger<InitializeTwoFactorAuthCommandHandler> logger)
        {
            _initializeTwoFactorAuthService = initializeTwoFactorAuthService;
            _initializeTwoFactorAuthRepository = initializeTwoFactorAuthRepository;
            _userRepository = userRepository;
            _dbContext = dbContext;
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

            // CONCURRENCY FIX: Use database-level locking to prevent race conditions
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var existingTwoFactorAuth = await _initializeTwoFactorAuthRepository.GetByUserIdWithLockAsync(command.UserId);
                if (existingTwoFactorAuth is not null && existingTwoFactorAuth.IsEnabled)
                {
                    throw new InvalidOperationException("2FA is already initialized for this user");
                }

                // If 2FA exists but is not enabled, delete it to start fresh
                if (existingTwoFactorAuth is not null && !existingTwoFactorAuth.IsEnabled)
                {
                    await _initializeTwoFactorAuthRepository.DeleteAsync(existingTwoFactorAuth.Id);
                    _logger.LogInformation("Deleted existing unenabled 2FA for user {UserId} to start fresh", command.UserId);
                }

                // Initialize 2FA (this creates the entity with IsEnabled = false)
                var twoFactorAuth = await _initializeTwoFactorAuthService.InitializeTwoFactorAuthAsync(user);
                
                // CONCURRENCY FIX: Set initial version for optimistic concurrency control
                twoFactorAuth.Version = 1;
                
                // Save the TwoFactorAuth entity to the database (with IsEnabled = false)
                await _initializeTwoFactorAuthRepository.CreateAsync(twoFactorAuth);
                
                // Generate QR code
                var qrCodeUrl = await _initializeTwoFactorAuthService.GenerateQrCodeAsync(twoFactorAuth, user.Email);

                // Commit the transaction
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("2FA initialized successfully for user {UserId} (saved to database with IsEnabled = false)", command.UserId);

                return new InitializeTwoFactorAuthResponse
                {
                    QrCodeUrl = qrCodeUrl,
                    SecretKey = twoFactorAuth.SecretKey
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
} 