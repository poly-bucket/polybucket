using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Core.Configuration;
using Core.Entities;
using Core.Exceptions;
using Core.Interfaces;
using Core.Models;
using Core.Models.Auth;

namespace Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ISystemSetupRepository _systemSetupRepository;
        private readonly AppSettings _appSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IEmailService emailService,
            ISystemSetupRepository systemSetupRepository,
            IOptions<AppSettings> appSettings,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _emailService = emailService;
            _systemSetupRepository = systemSetupRepository;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public async Task<ServiceResponse<bool>> IsFirstRunAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var isAdminConfigured = await _systemSetupRepository.IsAdminConfiguredAsync(cancellationToken);
                var isRoleConfigured = await _systemSetupRepository.IsRoleConfiguredAsync(cancellationToken);

                // If either admin or roles are not configured, it's considered a first run
                var isFirstRun = !isAdminConfigured || !isRoleConfigured;

                return ServiceResponse<bool>.Success(isFirstRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if first run");
                return ServiceResponse<bool>.Failure("Error checking if first run");
            }
        }

        public async Task<ServiceResponse<UserResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userResponse = await RegisterExAsync(request, cancellationToken);
                return ServiceResponse<UserResponse>.Success(userResponse);
            }
            catch (ValidationException ex)
            {
                return ServiceResponse<UserResponse>.ValidationFailure(ex.Message, ex.ValidationErrors);
            }
            catch (ResourceExistsException ex)
            {
                var errors = new Dictionary<string, List<string>>();
                AddValidationError(errors, ex.ResourceName, ex.Message);
                return ServiceResponse<UserResponse>.ValidationFailure(ex.Message, errors);
            }
            catch (AuthException ex)
            {
                return ServiceResponse<UserResponse>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Message} {InnerException}", ex.Message, ex.InnerException?.Message);
                throw new AuthException($"Error registering user: {ex.Message} Inner: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<UserResponse> RegisterExAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create a dictionary to collect validation errors
                var validationErrors = new Dictionary<string, List<string>>();

                // Validate username
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    AddValidationError(validationErrors, "Username", "Username is required");
                }

                // Validate email
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    AddValidationError(validationErrors, "Email", "Email is required");
                }
                else if (!IsValidEmail(request.Email))
                {
                    AddValidationError(validationErrors, "Email", "Email format is invalid");
                }

                // Validate password
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    AddValidationError(validationErrors, "Password", "Password is required");
                }
                else if (request.Password.Length < 6)
                {
                    AddValidationError(validationErrors, "Password", "Password must be at least 6 characters");
                }

                // Validate first name
                if (string.IsNullOrWhiteSpace(request.FirstName))
                {
                    AddValidationError(validationErrors, "FirstName", "First name is required");
                }

                // Validate last name
                if (string.IsNullOrWhiteSpace(request.LastName))
                {
                    AddValidationError(validationErrors, "LastName", "Last name is required");
                }

                // Throw validation exception if any errors
                if (validationErrors.Count > 0)
                {
                    throw new ValidationException("Please correct the validation errors", validationErrors);
                }

                // Check if username exists
                if (await _userRepository.UsernameExistsAsync(request.Username))
                {
                    throw new ResourceExistsException("Username is already taken", "Username", request.Username);
                }

                // Check if email exists
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    throw new ResourceExistsException("Email is already registered", "Email", request.Email);
                }

                // Create user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName ?? string.Empty,
                    LastName = request.LastName ?? string.Empty,
                    PasswordHash = _passwordHasher.HashPassword(request.Password),
                    PasswordSalt = string.Empty,
                    Roles = new List<string>(),
                    OrganizationIds = new List<string>(),
                    ProfilePictureUrl = string.Empty,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    EmailVerificationToken = Guid.NewGuid().ToString("N"),
                    EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(3),
                    PasswordResetToken = string.Empty,
                    PasswordResetTokenExpiry = null,
                    LockoutEnd = null,
                    LastLogin = null,
                    AccessFailedCount = 0,
                    IsLocked = false,
                    IsAdmin = request.IsAdmin,
                    IsEmailVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Save user
                try
                {
                    var createdUser = await _userRepository.AddAsync(user);

                    // Send verification email
                    if (_appSettings.Email.EnableEmailVerification)
                    {
                        await SendVerificationEmailAsync(createdUser, cancellationToken);
                    }

                    // Return user response
                    return MapToUserResponse(createdUser);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database error: {Message} InnerException: {InnerMessage}",
                        ex.Message, ex.InnerException?.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RegisterExAsync: {Message} InnerException: {InnerMessage}",
                    ex.Message, ex.InnerException?.Message);
                throw;
            }
        }

        public async Task<ServiceResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var authResponse = await LoginExAsync(request, cancellationToken);
                return ServiceResponse<AuthResponse>.Success(authResponse);
            }
            catch (ValidationException ex)
            {
                return ServiceResponse<AuthResponse>.ValidationFailure(ex.Message, ex.ValidationErrors);
            }
            catch (AuthException ex)
            {
                return ServiceResponse<AuthResponse>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user: {Message} {InnerException}", ex.Message, ex.InnerException?.Message);
                throw new AuthException($"Error logging in user: {ex.Message} Inner: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<ServiceResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var authResponse = await RefreshTokenExAsync(request, cancellationToken);
                return ServiceResponse<AuthResponse>.Success(authResponse);
            }
            catch (ValidationException ex)
            {
                return ServiceResponse<AuthResponse>.ValidationFailure(ex.Message, ex.ValidationErrors);
            }
            catch (AuthException ex)
            {
                return ServiceResponse<AuthResponse>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token: {Message} {InnerException}", ex.Message, ex.InnerException?.Message);
                throw new AuthException($"Error refreshing token: {ex.Message} Inner: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<ServiceResponse<bool>> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await VerifyEmailExAsync(token, cancellationToken);
                return ServiceResponse<bool>.Success(result);
            }
            catch (ValidationException ex)
            {
                return ServiceResponse<bool>.ValidationFailure(ex.Message, ex.ValidationErrors);
            }
            catch (AuthException ex)
            {
                return ServiceResponse<bool>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email: {Message} {InnerException}", ex.Message, ex.InnerException?.Message);
                throw new AuthException($"Error verifying email: {ex.Message} Inner: {ex.InnerException?.Message}", ex);
            }
        }

        private UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsAdmin = user.IsAdmin,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };
        }

        private async Task SendVerificationEmailAsync(User user, CancellationToken cancellationToken)
        {
            var verificationLink = $"{_appSettings.Frontend.BaseUrl}/verify-email?token={user.EmailVerificationToken}";
            var emailSubject = "Verify your email address";
            var emailBody = $"Please click the following link to verify your email address: {verificationLink}";

            await _emailService.SendAsync(user.Email, emailSubject, emailBody, cancellationToken);
        }

        private void AddValidationError(Dictionary<string, List<string>> errors, string key, string message)
        {
            if (!errors.ContainsKey(key))
            {
                errors[key] = new List<string>();
            }
            errors[key].Add(message);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsFirstRunExAsync(CancellationToken cancellationToken = default)
        {
            var isAdminConfigured = await _systemSetupRepository.IsAdminConfiguredAsync(cancellationToken);
            var isRoleConfigured = await _systemSetupRepository.IsRoleConfiguredAsync(cancellationToken);

            // If either admin or roles are not configured, it's considered a first run
            return !isAdminConfigured || !isRoleConfigured;
        }

        public async Task<AuthResponse> LoginExAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            // Get user by email or username
            User user;
            if (request.EmailOrUsername.Contains("@"))
            {
                user = await _userRepository.GetByEmailAsync(request.EmailOrUsername);
            }
            else
            {
                user = await _userRepository.GetByUsernameAsync(request.EmailOrUsername);
            }

            if (user == null)
            {
                throw new AuthException("Invalid email/username or password");
            }

            // Check if user is locked
            if (user.IsLocked)
            {
                if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
                {
                    throw new AuthException($"Account is locked until {user.LockoutEnd.Value}");
                }

                // If lockout period has passed, unlock the account
                user.IsLocked = false;
                user.LockoutEnd = null;
                user.AccessFailedCount = 0;
                await _userRepository.UpdateAsync(user);
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                user.AccessFailedCount++;

                // Lock account if too many failed attempts
                if (user.AccessFailedCount >= _appSettings.Security.MaxFailedAccessAttempts)
                {
                    user.IsLocked = true;
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(_appSettings.Security.LockoutDurationMinutes);
                }

                await _userRepository.UpdateAsync(user);

                if (user.IsLocked)
                {
                    throw new AuthException($"Account is locked until {user.LockoutEnd.Value}");
                }

                throw new AuthException("Invalid email/username or password");
            }

            // Reset failed attempts on successful login
            if (user.AccessFailedCount > 0)
            {
                user.AccessFailedCount = 0;
                await _userRepository.UpdateAsync(user);
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(_appSettings.Security.RefreshTokenExpiryDays);

            // Save refresh token
            user.RefreshTokens.Add(new Core.Entities.RefreshToken
            {
                Token = refreshToken,
                Created = DateTime.UtcNow,
                Expires = refreshTokenExpiry,
                CreatedByIp = "127.0.0.1", // TODO: Get from request context
                UserId = user.Id
            });

            await _userRepository.UpdateAsync(user);

            // Return response
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_appSettings.Security.AccessTokenExpiryMinutes),
                User = MapToUserResponse(user)
            };
        }

        public async Task<AuthResponse> RefreshTokenExAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                throw new ValidationException("Refresh token is required", new Dictionary<string, List<string>>
                {
                    { "RefreshToken", new List<string> { "Refresh token is required" } }
                });
            }

            // Get user by refresh token
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
            if (user == null)
            {
                throw new AuthException("Invalid refresh token");
            }

            // Find the refresh token
            var refreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);
            if (refreshToken == null)
            {
                throw new AuthException("Invalid refresh token");
            }

            // Check if token is expired
            if (refreshToken.IsExpired)
            {
                throw new AuthException("Refresh token has expired");
            }

            // Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(_appSettings.Security.RefreshTokenExpiryDays);

            // Remove old refresh token and add new one
            user.RefreshTokens.Remove(refreshToken);
            user.RefreshTokens.Add(new Core.Entities.RefreshToken
            {
                Token = newRefreshToken,
                Created = DateTime.UtcNow,
                Expires = refreshTokenExpiry,
                CreatedByIp = "127.0.0.1", // TODO: Get from request context
                UserId = user.Id
            });

            await _userRepository.UpdateAsync(user);

            // Return response
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_appSettings.Security.AccessTokenExpiryMinutes),
                User = MapToUserResponse(user)
            };
        }

        public async Task<bool> VerifyEmailExAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ValidationException("Token is required", new Dictionary<string, List<string>>
                {
                    { "Token", new List<string> { "Token is required" } }
                });
            }

            // Get user by email verification token
            var user = await _userRepository.GetByEmailAsync(token);
            if (user == null)
            {
                throw new AuthException("Invalid verification token");
            }

            // Check if token is expired
            if (user.EmailVerificationTokenExpiry <= DateTime.UtcNow)
            {
                throw new AuthException("Verification token has expired");
            }

            // Mark email as verified
            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;

            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<ServiceResponse<SystemSetup>> GetSetupStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var setup = await GetSetupStatusExAsync(cancellationToken);
                return ServiceResponse<SystemSetup>.Success(setup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting setup status");
                return ServiceResponse<SystemSetup>.Failure("Error getting setup status");
            }
        }

        public async Task<ServiceResponse<bool>> IsAdminConfiguredAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await IsAdminConfiguredExAsync(cancellationToken);
                return ServiceResponse<bool>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if admin is configured");
                return ServiceResponse<bool>.Failure("Error checking if admin is configured");
            }
        }

        public async Task<ServiceResponse<bool>> IsRoleConfiguredAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await IsRoleConfiguredExAsync(cancellationToken);
                return ServiceResponse<bool>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if roles are configured");
                return ServiceResponse<bool>.Failure("Error checking if roles are configured");
            }
        }

        public async Task<ServiceResponse<bool>> SetAdminConfiguredAsync(bool isConfigured, CancellationToken cancellationToken = default)
        {
            try
            {
                await SetAdminConfiguredExAsync(isConfigured, cancellationToken);
                return ServiceResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting admin configured status");
                return ServiceResponse<bool>.Failure("Error setting admin configured status");
            }
        }

        public async Task<ServiceResponse<bool>> SetRoleConfiguredAsync(bool isConfigured, CancellationToken cancellationToken = default)
        {
            try
            {
                await SetRoleConfiguredExAsync(isConfigured, cancellationToken);
                return ServiceResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting role configured status");
                return ServiceResponse<bool>.Failure("Error setting role configured status");
            }
        }

        public async Task<SystemSetup> GetSetupStatusExAsync(CancellationToken cancellationToken = default)
        {
            return await _systemSetupRepository.GetSetupStatusAsync(cancellationToken);
        }

        public async Task<bool> IsAdminConfiguredExAsync(CancellationToken cancellationToken = default)
        {
            return await _systemSetupRepository.IsAdminConfiguredAsync(cancellationToken);
        }

        public async Task<bool> IsRoleConfiguredExAsync(CancellationToken cancellationToken = default)
        {
            return await _systemSetupRepository.IsRoleConfiguredAsync(cancellationToken);
        }

        public async Task SetAdminConfiguredExAsync(bool isConfigured, CancellationToken cancellationToken = default)
        {
            await _systemSetupRepository.SetAdminConfiguredAsync(isConfigured, cancellationToken);
        }

        public async Task SetRoleConfiguredExAsync(bool isConfigured, CancellationToken cancellationToken = default)
        {
            await _systemSetupRepository.SetRoleConfiguredAsync(isConfigured, cancellationToken);
        }
    }
}