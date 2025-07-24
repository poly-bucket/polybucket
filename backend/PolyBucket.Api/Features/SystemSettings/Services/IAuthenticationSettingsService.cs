using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface IAuthenticationSettingsService
{
    Task<AuthenticationSettings> GetAuthenticationSettingsAsync();
    Task<bool> UpdateAuthenticationSettingsAsync(AuthenticationSettings settings);
    Task<bool> IsEmailLoginEnabledAsync();
    Task<bool> IsUsernameLoginEnabledAsync();
    Task<LoginMethod> GetLoginMethodAsync();
} 