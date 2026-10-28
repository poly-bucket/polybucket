using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Repository;

namespace PolyBucket.Api.Features.Authentication.Logout.Domain;

public class LogoutCommandHandler(
    IAuthenticationRepository authRepository,
    ILogger<LogoutCommandHandler> logger)
{
    private readonly IAuthenticationRepository _authRepository = authRepository;
    private readonly ILogger<LogoutCommandHandler> _logger = logger;

    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return;
        }

        var refreshToken = await _authRepository.GetRefreshTokenAsync(command.RefreshToken);
        if (refreshToken == null || !refreshToken.IsActive)
        {
            return;
        }

        await _authRepository.RevokeRefreshTokenAsync(
            command.RefreshToken,
            "User logged out",
            command.Client.IpAddress);

        _logger.LogInformation("User logged out; refresh token revoked");
    }
}
