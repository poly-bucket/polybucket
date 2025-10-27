using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.Domain
{
    public interface IOAuthPlugin : IPlugin
    {
        string ProviderName { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string AuthorizationEndpoint { get; }
        string TokenEndpoint { get; }
        string UserInfoEndpoint { get; }
        List<string> Scopes { get; }
        Dictionary<string, string> AdditionalParameters { get; }
        
        Task<OAuthAuthorizationResult> AuthorizeAsync(string code, string redirectUri);
        Task<OAuthUserInfo> GetUserInfoAsync(string accessToken);
        Task<OAuthTokenRefreshResult> RefreshTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string accessToken);
    }

    public class OAuthAuthorizationResult
    {
        public bool Success { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public string Scope { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }

    public class OAuthUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string ProfileUrl { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    public class OAuthTokenRefreshResult
    {
        public bool Success { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string Error { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
    }

    public class OAuthProviderConfig
    {
        public string ProviderName { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string AuthorizationEndpoint { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
        public string UserInfoEndpoint { get; set; } = string.Empty;
        public List<string> Scopes { get; set; } = new();
        public Dictionary<string, string> AdditionalParameters { get; set; } = new();
        public bool IsEnabled { get; set; }
        public bool AutoRegister { get; set; }
        public string DefaultRole { get; set; } = "User";
    }
}
