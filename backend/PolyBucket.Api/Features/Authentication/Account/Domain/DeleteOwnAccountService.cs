using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using UserEntity = PolyBucket.Api.Common.Models.User;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using PolyBucket.Api.Features.Authentication.Login.Repository;
using PolyBucket.Api.Features.Authentication.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.Authentication.Account.Domain;

public class DeleteOwnAccountService(
    PolyBucketDbContext context,
    IAuthenticationRepository authRepository,
    IPasswordHasher passwordHasher,
    ILoginTwoFactorAuthService loginTwoFactorAuthService,
    ILoginTwoFactorAuthRepository loginTwoFactorAuthRepository,
    IPermissionService permissionService,
    ILogger<DeleteOwnAccountService> logger)
{
    private readonly PolyBucketDbContext _context = context;
    private readonly IAuthenticationRepository _authRepository = authRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ILoginTwoFactorAuthService _loginTwoFactorAuthService = loginTwoFactorAuthService;
    private readonly ILoginTwoFactorAuthRepository _loginTwoFactorAuthRepository = loginTwoFactorAuthRepository;
    private readonly IPermissionService _permissionService = permissionService;
    private readonly ILogger<DeleteOwnAccountService> _logger = logger;

    public async Task<DeleteOwnAccountResult> ExecuteAsync(
        ClaimsPrincipal principal,
        DeleteAccountRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = principal.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return DeleteOwnAccountResult.Fail("Not authenticated");
        }

        if (!await _permissionService.HasPermissionAsync(userId, PermissionConstants.USER_DELETE_ACCOUNT))
        {
            return DeleteOwnAccountResult.Fail("You do not have permission to delete your account");
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Settings)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return DeleteOwnAccountResult.Fail("User not found");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return DeleteOwnAccountResult.Fail("Password is incorrect");
        }

        var twoFactorAuth = await _loginTwoFactorAuthRepository.GetByUserIdAsync(userId);
        if (twoFactorAuth?.IsEnabled == true)
        {
            if (string.IsNullOrWhiteSpace(request.TwoFactorToken) && string.IsNullOrWhiteSpace(request.BackupCode))
            {
                return DeleteOwnAccountResult.Fail("Two-factor authentication code is required");
            }

            var useBackup = !string.IsNullOrWhiteSpace(request.BackupCode);
            bool valid;
            if (useBackup)
            {
                valid = await _loginTwoFactorAuthService.ValidateBackupCodeAsync(twoFactorAuth, request.BackupCode!.Trim());
            }
            else
            {
                valid = await _loginTwoFactorAuthService.ValidateTokenAsync(twoFactorAuth, request.TwoFactorToken!.Trim());
            }

            if (!valid)
            {
                return DeleteOwnAccountResult.Fail("Invalid two-factor authentication code");
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        if (string.Equals(user.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var roleId = user.RoleId;
            if (roleId.HasValue)
            {
                var activeAdminCount = await _context.Users.CountAsync(
                    u => u.RoleId == roleId && u.CanLogin,
                    cancellationToken);
                if (activeAdminCount <= 1)
                {
                    return DeleteOwnAccountResult.Fail("Cannot delete the only administrator account");
                }
            }
        }

        await _authRepository.RevokeAllRefreshTokensForUserAsync(
            userId,
            "Account deleted",
            "127.0.0.1");

        AnonymizeUser(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} deleted their account (anonymized)", userId);
        return DeleteOwnAccountResult.Ok();
    }

    private void AnonymizeUser(UserEntity user)
    {
        var id = user.Id;
        user.Email = $"deleted-{id:N}@account-deleted.invalid";
        user.Username = $"deleted_{id:N}";
        user.FirstName = null;
        user.LastName = null;
        user.Bio = null;
        user.WebsiteUrl = null;
        user.TwitterUrl = null;
        user.InstagramUrl = null;
        user.YouTubeUrl = null;
        user.Country = null;
        user.Avatar = null;
        user.ProfilePictureUrl = null;
        user.CanLogin = false;
        user.IsProfilePublic = false;
        user.ShowEmail = false;
        user.ShowLastLogin = false;
        var salt = _passwordHasher.GenerateSalt();
        user.PasswordHash = _passwordHasher.HashPassword(Guid.NewGuid().ToString("N"), salt);
        user.Salt = salt;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedById = user.Id;
    }
}
