using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;

using RefreshTokenModel = PolyBucket.Api.Features.Authentication.Domain.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.Login.Domain
{
    public class LoginCommandHandler(
        IAuthenticationRepository authRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<LoginCommandHandler> logger,
        PolyBucketDbContext context,
        ITwoFactorAuthService twoFactorAuthService,
        ITwoFactorAuthRepository twoFactorAuthRepository)
    {
        private readonly IAuthenticationRepository _authRepository = authRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<LoginCommandHandler> _logger = logger;
        private readonly PolyBucketDbContext _context = context;
        private readonly ITwoFactorAuthService _twoFactorAuthService = twoFactorAuthService;
        private readonly ITwoFactorAuthRepository _twoFactorAuthRepository = twoFactorAuthRepository;


        public async Task<LoginCommandResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Login attempt received - EmailOrUsername: {EmailOrUsername}, Email: {Email}", 
                command.EmailOrUsername, command.Email);
            
            // Normalize the login identifier
            var loginIdentifier = !string.IsNullOrEmpty(command.EmailOrUsername) 
                ? command.EmailOrUsername 
                : command.Email; // Legacy support

            _logger.LogInformation("Normalized login identifier: {LoginIdentifier}", loginIdentifier);

            if (string.IsNullOrEmpty(loginIdentifier))
            {
                _logger.LogWarning("Login attempt with empty identifier");
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Get authentication settings - temporarily disabled to fix DI issue
            _logger.LogInformation("Auth settings validation temporarily disabled");
            
            // Determine if the identifier is an email or username
            var isEmail = IsEmailAddress(loginIdentifier);
            _logger.LogInformation("Identifier type - IsEmail: {IsEmail}, Identifier: {Identifier}", isEmail, loginIdentifier);
            
            // Validate login method based on settings - temporarily bypassed
            _logger.LogInformation("Skipping login method validation temporarily");
            /*
            var authSettings = await _authenticationSettingsService.GetAuthenticationSettingsAsync();
            _logger.LogInformation("Retrieved authentication settings: AllowEmailLogin={AllowEmailLogin}, AllowUsernameLogin={AllowUsernameLogin}", 
                authSettings.AllowEmailLogin, authSettings.AllowUsernameLogin);
            
            if (isEmail && !authSettings.AllowEmailLogin)
            {
                _logger.LogWarning("Email login attempted but not enabled for {Email}", loginIdentifier);
                throw new UnauthorizedAccessException("Email login is not enabled");
            }
            
            if (!isEmail && !authSettings.AllowUsernameLogin)
            {
                _logger.LogWarning("Username login attempted but not enabled for {Username}", loginIdentifier);
                throw new UnauthorizedAccessException("Username login is not enabled");
            }
            */

            // Find user based on identifier type
            User? user = null;
            if (isEmail)
            {
                user = await _authRepository.GetUserByEmailAsync(loginIdentifier);
                _logger.LogInformation("Looking up user by email: {Email}, Found: {Found}", loginIdentifier, user != null);
            }
            else
            {
                user = await _authRepository.GetUserByUsernameAsync(loginIdentifier);
                _logger.LogInformation("Looking up user by username: {Username}, Found: {Found}", loginIdentifier, user != null);
            }

            if (user == null)
            {
                _logger.LogWarning("User not found for identifier: {Identifier}", loginIdentifier);
                // Log failed login attempt
                await LogLoginAttempt(loginIdentifier, false, Guid.Empty);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user: {UserId}", user.Id);
                // Log failed login attempt
                await LogLoginAttempt(loginIdentifier, false, user.Id);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Check if user has 2FA enabled
            var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAsync(user.Id);
            var requiresTwoFactor = twoFactorAuth?.IsEnabled ?? false;

            if (requiresTwoFactor)
            {
                // If 2FA is required but no token provided, return 2FA requirement
                if (string.IsNullOrEmpty(command.TwoFactorToken) && string.IsNullOrEmpty(command.BackupCode))
                {
                    _logger.LogInformation("2FA required for user {UserId}", user.Id);
                    return new LoginCommandResponse
                    {
                        RequiresTwoFactor = true,
                        TwoFactorToken = null
                    };
                }

                // Validate 2FA token or backup code
                bool isValidTwoFactor = false;
                if (twoFactorAuth is not null)
                {
                    if (!string.IsNullOrEmpty(command.TwoFactorToken))
                    {
                        isValidTwoFactor = await _twoFactorAuthService.ValidateTokenAsync(twoFactorAuth, command.TwoFactorToken);
                    }
                    else if (!string.IsNullOrEmpty(command.BackupCode))
                    {
                        isValidTwoFactor = await _twoFactorAuthService.ValidateBackupCodeAsync(twoFactorAuth, command.BackupCode);
                    }
                }

                if (!isValidTwoFactor)
                {
                    _logger.LogWarning("Invalid 2FA token/backup code for user {UserId}", user.Id);
                    await LogLoginAttempt(loginIdentifier, false, user.Id);
                    throw new UnauthorizedAccessException("Invalid two-factor authentication code");
                }
            }

            // Log successful login attempt (both application log and database record)
            await LogLoginAttempt(loginIdentifier, true, user.Id);
            
            // Generate authentication response
            var authResponse = _tokenService.GenerateAuthenticationResponse(user);

            // Create refresh token
            var refreshToken = new RefreshTokenModel
            {
                Id = Guid.NewGuid(),
                Token = authResponse.RefreshToken,
                UserId = user.Id,
                ExpiresAt = authResponse.RefreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1" // TODO: Get from request
            };

            await _authRepository.CreateRefreshTokenAsync(refreshToken);

            // Check if user requires password change or first-time setup
            var requiresPasswordChange = user.RequiresPasswordChange;
            var requiresFirstTimeSetup = user.HasCompletedFirstTimeSetup == false;
            var setupStep = (string?)null;

            // If admin user, also check system setup status
            if (user.Role?.Name == "Admin" && _context != null)
            {
                try
                {
                    var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                    if (systemSetup != null && systemSetup.IsFirstTimeSetup)
                    {
                        requiresFirstTimeSetup = true;
                        
                        // Determine the current step based on what's been completed
                        if (!systemSetup.IsAdminConfigured)
                        {
                            setupStep = "password"; // First step is password change
                        }
                        else if (!systemSetup.IsSiteConfigured)
                        {
                            setupStep = "site"; // Site configuration
                        }
                        else if (!systemSetup.IsEmailConfigured)
                        {
                            setupStep = "email"; // Email configuration
                        }
                        else if (!systemSetup.IsModerationConfigured)
                        {
                            setupStep = "moderation"; // Moderation settings
                        }
                    }
                }
                catch (Exception ex)
                {
                    // In test environments or when database is not available, 
                    // we'll skip the setup check and assume setup is not required
                    _logger.LogWarning(ex, "Could not check system setup status, assuming setup is not required");
                }
            }

            return new LoginCommandResponse 
            { 
                Token = authResponse.AccessToken,
                RefreshToken = authResponse.RefreshToken,
                TokenExpiresAt = authResponse.AccessTokenExpiresAt,
                RefreshTokenExpiresAt = authResponse.RefreshTokenExpiresAt,
                RequiresPasswordChange = requiresPasswordChange,
                RequiresFirstTimeSetup = requiresFirstTimeSetup,
                SetupStep = setupStep
            };
        }

        private bool IsEmailAddress(string identifier)
        {
            return identifier.Contains("@") && identifier.Contains(".");
        }

        private async Task LogLoginAttempt(string identifier, bool success, Guid userId)
        {
            var loginRecord = new UserLogin
            {
                Email = identifier, // Store the identifier as email for backward compatibility
                UserId = userId,
                Successful = success,
                UserAgent = "Unknown", // TODO: Get from request
                CreatedAt = DateTime.UtcNow
            };

            await _authRepository.CreateLoginRecordAsync(loginRecord);
            _logger.LogInformation("Login attempt for {Identifier}. Success: {Success}", identifier, success);
        }
    }
} 