using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Logout.Repository;

namespace PolyBucket.Api.Features.Authentication.Logout.Domain;

public class LogoutCommandHandler(
    ILogoutRepository logoutRepository,
    ILogger<LogoutCommandHandler> logger)
{
    private readonly ILogoutRepository _logoutRepository = logoutRepository;
    private readonly ILogger<LogoutCommandHandler> _logger = logger;

    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return;
        }

        var refreshToken = await _logoutRepository.GetRefreshTokenAsync(command.RefreshToken);
        if (refreshToken == null || !refreshToken.IsActive)
        {
            return;
        }

        await _logoutRepository.RevokeRefreshTokenAsync(
            command.RefreshToken,
            "User logged out",
            command.Client.IpAddress);

        _logger.LogInformation("User logged out; refresh token revoked");
    }
}
