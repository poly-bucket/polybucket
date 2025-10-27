using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Plugins.Domain;
using PluginComponent = PolyBucket.Api.Common.Plugins.PluginComponent;
using PluginHook = PolyBucket.Api.Common.Plugins.PluginHook;
using PluginSetting = PolyBucket.Api.Common.Plugins.PluginSetting;

namespace PolyBucket.Api.Features.Plugins.Examples
{
    public class DiscordOAuthPlugin : IPlugin, IOAuthPlugin
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DiscordOAuthPlugin> _logger;
        private readonly DiscordOAuthSettings _settings;

        public DiscordOAuthPlugin(
            HttpClient httpClient,
            ILogger<DiscordOAuthPlugin> logger,
            IOptions<DiscordOAuthSettings> settings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;
        }

        public string Id => "discord-oauth-plugin";
        public string Name => "Discord OAuth";
        public string Version => "1.0.0";
        public string Author => "PolyBucket Community";
        public string Description => "Discord account integration and community features";

        public string ProviderName => "discord";
        public string ClientId => _settings.ClientId;
        public string ClientSecret => _settings.ClientSecret;
        public string AuthorizationEndpoint => "https://discord.com/api/oauth2/authorize";
        public string TokenEndpoint => "https://discord.com/api/oauth2/token";
        public string UserInfoEndpoint => "https://discord.com/api/users/@me";
        public List<string> Scopes => new() { "identify", "email", "guilds" };
        public Dictionary<string, string> AdditionalParameters => new()
        {
            ["prompt"] = "consent"
        };

        public IEnumerable<PluginComponent> FrontendComponents => new List<PluginComponent>
        {
            new PluginComponent
            {
                Id = "discord-login-button",
                Name = "Discord Login Button",
                ComponentPath = "plugins/discord-oauth/DiscordLoginButton",
                Type = ComponentType.Widget,
                Hooks = new List<PluginHook>
                {
                    new PluginHook
                    {
                        HookName = "login-buttons",
                        ComponentId = "discord-login-button",
                        Priority = 100
                    }
                }
            },
            new PluginComponent
            {
                Id = "discord-user-profile",
                Name = "Discord User Profile",
                ComponentPath = "plugins/discord-oauth/DiscordUserProfile",
                Type = ComponentType.Widget,
                Hooks = new List<PluginHook>
                {
                    new PluginHook
                    {
                        HookName = "user-profile",
                        ComponentId = "discord-user-profile",
                        Priority = 100
                    }
                }
            }
        };

        public PluginMetadata Metadata => new PluginMetadata
        {
            MinimumAppVersion = "1.0.0",
            RequiredPermissions = new List<string> { "oauth.provide", "user.authenticate" },
            OptionalPermissions = new List<string> { "admin.oauth" },
            Settings = new Dictionary<string, PluginSetting>
            {
                ["clientId"] = new PluginSetting
                {
                    Name = "Discord Client ID",
                    Description = "Discord application client ID",
                    Type = PluginSettingType.String,
                    DefaultValue = "",
                    Required = true
                },
                ["clientSecret"] = new PluginSetting
                {
                    Name = "Discord Client Secret",
                    Description = "Discord application client secret",
                    Type = PluginSettingType.String,
                    DefaultValue = "",
                    Required = true
                },
                ["autoRegister"] = new PluginSetting
                {
                    Name = "Auto Register Users",
                    Description = "Automatically register new Discord users",
                    Type = PluginSettingType.Boolean,
                    DefaultValue = true,
                    Required = false
                },
                ["defaultRole"] = new PluginSetting
                {
                    Name = "Default Role",
                    Description = "Default role for new Discord users",
                    Type = PluginSettingType.String,
                    DefaultValue = "User",
                    Required = false
                }
            },
            Lifecycle = new PluginLifecycle
            {
                AutoStart = true,
                CanDisable = true,
                CanUninstall = true
            }
        };

        public async Task<OAuthAuthorizationResult> AuthorizeAsync(string code, string redirectUri)
        {
            try
            {
                _logger.LogInformation("Authorizing Discord user with code");

                var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("client_secret", ClientSecret),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri)
                });

                var response = await _httpClient.PostAsync(TokenEndpoint, tokenRequest);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Discord token exchange failed: {StatusCode} - {Content}", 
                        response.StatusCode, content);
                    
                    return new OAuthAuthorizationResult
                    {
                        Success = false,
                        Error = "token_exchange_failed",
                        ErrorDescription = "Failed to exchange authorization code for access token"
                    };
                }

                var tokenResponse = JsonSerializer.Deserialize<DiscordTokenResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenResponse == null)
                {
                    return new OAuthAuthorizationResult
                    {
                        Success = false,
                        Error = "invalid_response",
                        ErrorDescription = "Invalid response from Discord token endpoint"
                    };
                }

                _logger.LogInformation("Successfully authorized Discord user");

                return new OAuthAuthorizationResult
                {
                    Success = true,
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    TokenType = tokenResponse.TokenType,
                    ExpiresIn = tokenResponse.ExpiresIn,
                    Scope = tokenResponse.Scope
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authorizing Discord user");
                return new OAuthAuthorizationResult
                {
                    Success = false,
                    Error = "authorization_error",
                    ErrorDescription = ex.Message
                };
            }
        }

        public async Task<OAuthUserInfo> GetUserInfoAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Getting Discord user information");

                var request = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Discord user info request failed: {StatusCode} - {Content}", 
                        response.StatusCode, content);
                    throw new HttpRequestException($"Failed to get user info: {response.StatusCode}");
                }

                var userResponse = JsonSerializer.Deserialize<DiscordUserResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (userResponse == null)
                {
                    throw new InvalidOperationException("Invalid response from Discord user endpoint");
                }

                _logger.LogInformation("Successfully retrieved Discord user information for {Username}", 
                    userResponse.Username);

                return new OAuthUserInfo
                {
                    Id = userResponse.Id,
                    Username = userResponse.Username,
                    Email = userResponse.Email ?? string.Empty,
                    DisplayName = userResponse.GlobalName ?? userResponse.Username,
                    AvatarUrl = userResponse.Avatar != null 
                        ? $"https://cdn.discordapp.com/avatars/{userResponse.Id}/{userResponse.Avatar}.png"
                        : string.Empty,
                    ProfileUrl = $"https://discord.com/users/{userResponse.Id}",
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["discriminator"] = userResponse.Discriminator,
                        ["verified"] = userResponse.Verified,
                        ["locale"] = userResponse.Locale ?? string.Empty,
                        ["mfa_enabled"] = userResponse.MfaEnabled,
                        ["premium_type"] = userResponse.PremiumType
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Discord user information");
                throw;
            }
        }

        public async Task<OAuthTokenRefreshResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Refreshing Discord token");

                var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("client_secret", ClientSecret),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refreshToken)
                });

                var response = await _httpClient.PostAsync(TokenEndpoint, tokenRequest);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Discord token refresh failed: {StatusCode} - {Content}", 
                        response.StatusCode, content);
                    
                    return new OAuthTokenRefreshResult
                    {
                        Success = false,
                        Error = "refresh_failed",
                        ErrorDescription = "Failed to refresh access token"
                    };
                }

                var tokenResponse = JsonSerializer.Deserialize<DiscordTokenResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenResponse == null)
                {
                    return new OAuthTokenRefreshResult
                    {
                        Success = false,
                        Error = "invalid_response",
                        ErrorDescription = "Invalid response from Discord token endpoint"
                    };
                }

                _logger.LogInformation("Successfully refreshed Discord token");

                return new OAuthTokenRefreshResult
                {
                    Success = true,
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    ExpiresIn = tokenResponse.ExpiresIn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing Discord token");
                return new OAuthTokenRefreshResult
                {
                    Success = false,
                    Error = "refresh_error",
                    ErrorDescription = ex.Message
                };
            }
        }

        public async Task<bool> ValidateTokenAsync(string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Discord token");
                return false;
            }
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing Discord OAuth plugin");
            await Task.CompletedTask;
        }

        public async Task UnloadAsync()
        {
            _logger.LogInformation("Unloading Discord OAuth plugin");
            await Task.CompletedTask;
        }
    }

    public class DiscordOAuthSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public bool AutoRegister { get; set; } = true;
        public string DefaultRole { get; set; } = "User";
    }

    public class DiscordTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public string Scope { get; set; } = string.Empty;
    }

    public class DiscordUserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Discriminator { get; set; } = string.Empty;
        public string? GlobalName { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
        public bool Verified { get; set; }
        public string? Locale { get; set; }
        public bool MfaEnabled { get; set; }
        public int PremiumType { get; set; }
    }
}
