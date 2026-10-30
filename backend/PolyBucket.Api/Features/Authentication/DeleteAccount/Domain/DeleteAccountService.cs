using System.Security.Claims;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Authentication.DeleteAccount.Repository;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using PolyBucket.Api.Features.Authentication.Login.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using UserEntity = PolyBucket.Database.Entities.Models.User;

namespace PolyBucket.Api.Features.Authentication.DeleteAccount.Domain;

public interface IDeleteAccountService
{
    Task<DeleteAccountResult> ExecuteAsync(ClaimsPrincipal principal, DeleteAccountRequest request, CancellationToken cancellationToken);
}

public class DeleteAccountService(
    IDeleteAccountRepository repository,
    IPasswordHasher passwordHasher,
    ILoginTwoFactorAuthService loginTwoFactorAuthService,
    ILoginTwoFactorAuthRepository loginTwoFactorAuthRepository,
    IPermissionService permissionService,
    ILogger<DeleteAccountService> logger) : IDeleteAccountService
{
    private readonly IDeleteAccountRepository _repository = repository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ILoginTwoFactorAuthService _loginTwoFactorAuthService = loginTwoFactorAuthService;
    private readonly ILoginTwoFactorAuthRepository _loginTwoFactorAuthRepository = loginTwoFactorAuthRepository;
    private readonly IPermissionService _permissionService = permissionService;
    private readonly ILogger<DeleteAccountService> _logger = logger;

    public async Task<DeleteAccountResult> ExecuteAsync(
        ClaimsPrincipal principal,
        DeleteAccountRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = principal.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return DeleteAccountResult.Fail("Not authenticated");
        }

        if (!await _permissionService.HasPermissionAsync(userId, PermissionConstants.USER_DELETE_ACCOUNT))
        {
            return DeleteAccountResult.Fail("You do not have permission to delete your account");
        }

        var user = await _repository.GetUserForDeleteAsync(userId, cancellationToken);
        if (user == null)
        {
            return DeleteAccountResult.Fail("User not found");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return DeleteAccountResult.Fail("Password is incorrect");
        }

        var twoFactorAuth = await _loginTwoFactorAuthRepository.GetByUserIdAsync(userId);
        if (twoFactorAuth?.IsEnabled == true)
        {
            if (string.IsNullOrWhiteSpace(request.TwoFactorToken) && string.IsNullOrWhiteSpace(request.BackupCode))
            {
                return DeleteAccountResult.Fail("Two-factor authentication code is required");
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
                return DeleteAccountResult.Fail("Invalid two-factor authentication code");
            }

            await _repository.SaveChangesAsync(cancellationToken);
        }

        if (string.Equals(user.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var roleId = user.RoleId;
            if (roleId.HasValue)
            {
                var activeAdminCount = await _repository.CountActiveAdminsWithRoleAsync(roleId.Value, cancellationToken);
                if (activeAdminCount <= 1)
                {
                    return DeleteAccountResult.Fail("Cannot delete the only administrator account");
                }
            }
        }

        await _repository.RevokeAllRefreshTokensForUserAsync(userId, "Account deleted", "127.0.0.1");
        AnonymizeUser(user);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} deleted their account (anonymized)", userId);
        return DeleteAccountResult.Ok();
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
