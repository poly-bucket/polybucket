using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.RefreshToken.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.RefreshToken.Domain
{
    public class RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger = logger;

        public async Task<RefreshTokenCommandResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            // Get refresh token
            var refreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(command.RefreshToken);
            if (refreshToken == null || !refreshToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            // Get user
            var user = await _refreshTokenRepository.GetUserByEmailAsync(refreshToken.User.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // Revoke the current refresh token
            await _refreshTokenRepository.RevokeRefreshTokenAsync(command.RefreshToken, "Replaced by new token", command.Client.IpAddress);

            // Generate new authentication response
            var authResponse = _tokenService.GenerateAuthenticationResponse(user);

            // Create new refresh token
            var newRefreshToken = new PolyBucket.Database.Entities.Authentication.RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = authResponse.RefreshToken,
                UserId = user.Id,
                ExpiresAt = authResponse.RefreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = command.Client.IpAddress,
                ReplacedByToken = authResponse.RefreshToken
            };

            await _refreshTokenRepository.CreateRefreshTokenAsync(newRefreshToken);

            _logger.LogInformation("Token refreshed successfully for user: {Email}", user.Email);

            return new RefreshTokenCommandResponse
            {
                Authentication = RefreshTokenAuthenticationMapper.From(authResponse)
            };
        }
    }
} 