using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.SystemSettings.Services;

namespace PolyBucket.Api.Features.Authentication.Services;

public class PasswordPolicyService(IAuthenticationSettingsService authenticationSettingsService) : IPasswordPolicyService
{
    private const string SpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";

  public async Task<(bool IsValid, string Message)> ValidateAsync(string password, CancellationToken cancellationToken = default)
    {
        var settings = await authenticationSettingsService.GetAuthenticationSettingsAsync();
        var minLength = settings.PasswordMinLength < 6 ? 6 : settings.PasswordMinLength;

        if (string.IsNullOrEmpty(password) || password.Length < minLength)
        {
            return (false, $"Password must be at least {minLength} characters long.");
        }

        if (!settings.RequireStrongPasswords)
        {
            return (true, string.Empty);
        }

        if (!password.Any(char.IsUpper))
        {
            return (false, "Password must contain at least one uppercase letter.");
        }

        if (!password.Any(char.IsLower))
        {
            return (false, "Password must contain at least one lowercase letter.");
        }

        if (!password.Any(char.IsDigit))
        {
            return (false, "Password must contain at least one number.");
        }

        if (!password.Any(c => SpecialCharacters.Contains(c)))
        {
            return (false, "Password must contain at least one special character.");
        }

        return (true, string.Empty);
    }
}