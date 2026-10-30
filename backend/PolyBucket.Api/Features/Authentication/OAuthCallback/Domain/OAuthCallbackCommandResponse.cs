namespace PolyBucket.Api.Features.Authentication.OAuthCallback.Domain;

public class OAuthCallbackAuthenticationResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public OAuthCallbackUserInfo User { get; set; } = new();
}

public class OAuthCallbackUserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Avatar { get; set; }
}

internal static class OAuthCallbackAuthenticationMapper
{
    public static OAuthCallbackAuthenticationResponse From(PolyBucket.Api.Features.Authentication.Domain.AuthenticationResponse source)
    {
        return new OAuthCallbackAuthenticationResponse
        {
            AccessToken = source.AccessToken,
            RefreshToken = source.RefreshToken,
            AccessTokenExpiresAt = source.AccessTokenExpiresAt,
            RefreshTokenExpiresAt = source.RefreshTokenExpiresAt,
            User = new OAuthCallbackUserInfo
            {
                Id = source.User.Id,
                Email = source.User.Email,
                Username = source.User.Username,
                FirstName = source.User.FirstName,
                LastName = source.User.LastName,
                Role = source.User.Role,
                IsEmailVerified = source.User.IsEmailVerified,
                CreatedAt = source.User.CreatedAt,
                Avatar = source.User.Avatar,
            },
        };
    }
}

public class OAuthCallbackCommandResponse
{
    public OAuthCallbackAuthenticationResponse Authentication { get; set; } = new();
    public bool IsNewUser { get; set; }
    public string Provider { get; set; } = string.Empty;
}
