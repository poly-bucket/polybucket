using PolyBucket.Api.Features.Authentication.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Services
{
    public interface IOAuthService
    {
        Task<string> GetAuthorizationUrlAsync(string provider, string redirectUri, string state);
        Task<OAuthUserInfo> GetUserInfoAsync(string provider, string code, string redirectUri);
        Task<AuthenticationResponse> AuthenticateWithOAuthAsync(string provider, string code, string redirectUri, string ipAddress);
    }

    public class OAuthUserInfo
    {
        public string Provider { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Picture { get; set; }
    }
} 