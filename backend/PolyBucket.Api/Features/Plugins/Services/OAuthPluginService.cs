using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.Services
{
    public class OAuthPluginService
    {
        private readonly ILogger<OAuthPluginService> _logger;
        private readonly Dictionary<string, IOAuthPlugin> _oauthProviders = new();

        public OAuthPluginService(ILogger<OAuthPluginService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> RegisterOAuthProviderAsync(IOAuthPlugin oauthPlugin)
        {
            try
            {
                _logger.LogInformation("Registering OAuth provider {ProviderName} from plugin {PluginId}", 
                    oauthPlugin.ProviderName, oauthPlugin.Id);

                if (_oauthProviders.ContainsKey(oauthPlugin.ProviderName))
                {
                    _logger.LogWarning("OAuth provider {ProviderName} is already registered", oauthPlugin.ProviderName);
                    return false;
                }

                _oauthProviders[oauthPlugin.ProviderName] = oauthPlugin;
                
                _logger.LogInformation("Successfully registered OAuth provider {ProviderName}", oauthPlugin.ProviderName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering OAuth provider {ProviderName}", oauthPlugin.ProviderName);
                return false;
            }
        }

        public async Task<bool> UnregisterOAuthProviderAsync(string providerName)
        {
            try
            {
                if (_oauthProviders.Remove(providerName))
                {
                    _logger.LogInformation("Successfully unregistered OAuth provider {ProviderName}", providerName);
                    return true;
                }
                
                _logger.LogWarning("OAuth provider {ProviderName} was not registered", providerName);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering OAuth provider {ProviderName}", providerName);
                return false;
            }
        }

        public async Task<OAuthAuthorizationResult> AuthorizeAsync(string providerName, string code, string redirectUri)
        {
            try
            {
                if (!_oauthProviders.TryGetValue(providerName, out var oauthPlugin))
                {
                    return new OAuthAuthorizationResult
                    {
                        Success = false,
                        Error = "invalid_provider",
                        ErrorDescription = $"OAuth provider '{providerName}' is not registered"
                    };
                }

                _logger.LogInformation("Authorizing user with OAuth provider {ProviderName}", providerName);
                
                var result = await oauthPlugin.AuthorizeAsync(code, redirectUri);
                
                if (result.Success)
                {
                    _logger.LogInformation("Successfully authorized user with OAuth provider {ProviderName}", providerName);
                }
                else
                {
                    _logger.LogWarning("Failed to authorize user with OAuth provider {ProviderName}: {Error}", 
                        providerName, result.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authorizing user with OAuth provider {ProviderName}", providerName);
                return new OAuthAuthorizationResult
                {
                    Success = false,
                    Error = "authorization_error",
                    ErrorDescription = ex.Message
                };
            }
        }

        public async Task<OAuthUserInfo> GetUserInfoAsync(string providerName, string accessToken)
        {
            try
            {
                if (!_oauthProviders.TryGetValue(providerName, out var oauthPlugin))
                {
                    throw new InvalidOperationException($"OAuth provider '{providerName}' is not registered");
                }

                _logger.LogInformation("Getting user info from OAuth provider {ProviderName}", providerName);
                
                var userInfo = await oauthPlugin.GetUserInfoAsync(accessToken);
                
                _logger.LogInformation("Successfully retrieved user info from OAuth provider {ProviderName}", providerName);
                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info from OAuth provider {ProviderName}", providerName);
                throw;
            }
        }

        public async Task<OAuthTokenRefreshResult> RefreshTokenAsync(string providerName, string refreshToken)
        {
            try
            {
                if (!_oauthProviders.TryGetValue(providerName, out var oauthPlugin))
                {
                    return new OAuthTokenRefreshResult
                    {
                        Success = false,
                        Error = "invalid_provider",
                        ErrorDescription = $"OAuth provider '{providerName}' is not registered"
                    };
                }

                _logger.LogInformation("Refreshing token for OAuth provider {ProviderName}", providerName);
                
                var result = await oauthPlugin.RefreshTokenAsync(refreshToken);
                
                if (result.Success)
                {
                    _logger.LogInformation("Successfully refreshed token for OAuth provider {ProviderName}", providerName);
                }
                else
                {
                    _logger.LogWarning("Failed to refresh token for OAuth provider {ProviderName}: {Error}", 
                        providerName, result.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token for OAuth provider {ProviderName}", providerName);
                return new OAuthTokenRefreshResult
                {
                    Success = false,
                    Error = "refresh_error",
                    ErrorDescription = ex.Message
                };
            }
        }

        public async Task<bool> ValidateTokenAsync(string providerName, string accessToken)
        {
            try
            {
                if (!_oauthProviders.TryGetValue(providerName, out var oauthPlugin))
                {
                    return false;
                }

                return await oauthPlugin.ValidateTokenAsync(accessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token for OAuth provider {ProviderName}", providerName);
                return false;
            }
        }

        public List<OAuthProviderInfo> GetRegisteredProviders()
        {
            var providers = new List<OAuthProviderInfo>();
            
            foreach (var kvp in _oauthProviders)
            {
                var plugin = kvp.Value;
                providers.Add(new OAuthProviderInfo
                {
                    ProviderName = plugin.ProviderName,
                    PluginId = plugin.Id,
                    PluginName = plugin.Name,
                    ClientId = plugin.ClientId,
                    AuthorizationEndpoint = plugin.AuthorizationEndpoint,
                    Scopes = plugin.Scopes,
                    IsEnabled = true // TODO: Check actual enabled status
                });
            }

            return providers;
        }

        public bool IsProviderRegistered(string providerName)
        {
            return _oauthProviders.ContainsKey(providerName);
        }
    }

    public class OAuthProviderInfo
    {
        public string ProviderName { get; set; } = string.Empty;
        public string PluginId { get; set; } = string.Empty;
        public string PluginName { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string AuthorizationEndpoint { get; set; } = string.Empty;
        public List<string> Scopes { get; set; } = new();
        public bool IsEnabled { get; set; }
    }
}
