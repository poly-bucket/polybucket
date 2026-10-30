using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.ChangePassword.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Authentication.ChangePassword.Domain;

public class ChangePasswordCommandHandler(
    IChangePasswordRepository changePasswordRepository,
    IPasswordHasher passwordHasher,
    IAccountEmailService accountEmailService,
    ILogger<ChangePasswordCommandHandler> logger,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly IChangePasswordRepository _changePasswordRepository = changePasswordRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IAccountEmailService _accountEmailService = accountEmailService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = "User not authenticated",
                    RequiresFirstTimeSetup = false
                };
            }

            var user = await _changePasswordRepository.GetUserForPasswordChangeAsync(userId, cancellationToken);
            if (user == null)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = "User not found",
                    RequiresFirstTimeSetup = false
                };
            }

            if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = "Current password is incorrect",
                    RequiresFirstTimeSetup = false
                };
            }

            var salt = _passwordHasher.GenerateSalt();
            var passwordHash = _passwordHasher.HashPassword(command.NewPassword, salt);

            user.PasswordHash = passwordHash;
            user.Salt = salt;
            user.UpdatedAt = DateTime.UtcNow;
            user.RequiresPasswordChange = false;

            var refreshTokens = await _changePasswordRepository.GetActiveRefreshTokensAsync(userId, cancellationToken);
            foreach (var token in refreshTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.ReasonRevoked = "Password changed";
                token.RevokedByIp = command.Client.IpAddress;
            }

            if (user.Role?.Name == "Admin" && !user.HasCompletedFirstTimeSetup)
            {
                await _changePasswordRepository.MarkAdminConfiguredAsync(userId, cancellationToken);
            }

            await _changePasswordRepository.SaveChangesAsync(cancellationToken);
            await _accountEmailService.SendPasswordChangedAsync(user, command.Client, cancellationToken: cancellationToken);

            _logger.LogInformation("Password changed successfully for user {UserId}", userId);

            return new ChangePasswordResponse
            {
                Success = true,
                Message = "Password changed successfully",
                RequiresFirstTimeSetup = user.Role?.Name == "Admin" && !user.HasCompletedFirstTimeSetup
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change password for user");
            return new ChangePasswordResponse
            {
                Success = false,
                Message = "Failed to change password. Please try again.",
                RequiresFirstTimeSetup = false
            };
        }
    }
}
