using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.RefreshToken.Domain
{
    public class RefreshTokenCommandHandler
    {
        private readonly IAuthenticationRepository _authRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            IAuthenticationRepository authRepository,
            ITokenService tokenService,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _authRepository = authRepository;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<RefreshTokenCommandResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            // Get refresh token
            var refreshToken = await _authRepository.GetRefreshTokenAsync(command.RefreshToken);
            if (refreshToken == null || !refreshToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            // Get user
            var user = await _authRepository.GetUserByEmailAsync(refreshToken.User.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // Revoke the current refresh token
            await _authRepository.RevokeRefreshTokenAsync(command.RefreshToken, "Replaced by new token", "127.0.0.1");

            // Generate new authentication response
            var authResponse = _tokenService.GenerateAuthenticationResponse(user);

            // Create new refresh token
            var newRefreshToken = new PolyBucket.Api.Features.Authentication.Domain.RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = authResponse.RefreshToken,
                UserId = user.Id,
                ExpiresAt = authResponse.RefreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1", // TODO: Get from request
                ReplacedByToken = authResponse.RefreshToken
            };

            await _authRepository.CreateRefreshTokenAsync(newRefreshToken);

            _logger.LogInformation("Token refreshed successfully for user: {Email}", user.Email);

            return new RefreshTokenCommandResponse
            {
                Authentication = authResponse
            };
        }
    }
} 