using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using RefreshTokenModel = PolyBucket.Api.Features.Authentication.Domain.RefreshToken;

namespace PolyBucket.Api.Features.Authentication.Login.Domain
{
    public class LoginCommandHandler
    {
        private readonly IAuthenticationRepository _authRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IAuthenticationRepository authRepository,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<LoginCommandHandler> logger)
        {
            _authRepository = authRepository;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginCommandResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await _authRepository.GetUserByEmailAsync(command.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            {
                // Log failed login attempt
                await LogLoginAttempt(command.Email, false);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Log successful login attempt
            await LogLoginAttempt(command.Email, true, user.Id);
            
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

            return new LoginCommandResponse { Token = authResponse.AccessToken };
        }

        private async Task LogLoginAttempt(string email, bool success, Guid? userId = null)
        {
            var loginRecord = new UserLogin
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserId = userId ?? Guid.Empty,
                Successful = success,
                UserAgent = "Unknown", // TODO: Get from request
                CreatedAt = DateTime.UtcNow
            };

            await _authRepository.CreateLoginRecordAsync(loginRecord);
            _logger.LogInformation("Login attempt for {Email}. Success: {Success}", email, success);
        }
    }
} 