using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace PolyBucket.Marketplace.Api.Services
{
    /// <summary>
    /// Service for handling authentication and GitHub OAuth integration
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponse> AuthenticateWithGitHubAsync(string code, string? state);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task<UserProfile?> GetUserProfileAsync(string userId);
        Task<UserProfile?> GetUserProfileByGitHubIdAsync(long githubId);
        Task<bool> RevokeTokenAsync(string token);
        Task<string> GenerateJwtTokenAsync(MarketplaceUser user);
        Task<string> GenerateRefreshTokenAsync();
        Task<GitHubUserInfo?> GetGitHubUserInfoAsync(string accessToken);
        Task<GitHubTokenResponse?> ExchangeCodeForTokenAsync(string code);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<MarketplaceUser> _userManager;
        private readonly SignInManager<MarketplaceUser> _signInManager;
        private readonly MarketplaceDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly HttpClient _httpClient;

        public AuthService(
            UserManager<MarketplaceUser> userManager,
            SignInManager<MarketplaceUser> signInManager,
            MarketplaceDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            HttpClient httpClient)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Authenticate user with GitHub OAuth code
        /// </summary>
        public async Task<AuthResponse> AuthenticateWithGitHubAsync(string code, string? state)
        {
            try
            {
                _logger.LogInformation("Starting GitHub OAuth authentication");

                // Exchange code for access token
                var tokenResponse = await ExchangeCodeForTokenAsync(code);
                if (tokenResponse == null)
                {
                    _logger.LogWarning("Failed to exchange code for token");
                    return new AuthResponse { Success = false, Error = "Failed to exchange code for token" };
                }

                // Get user info from GitHub
                var githubUser = await GetGitHubUserInfoAsync(tokenResponse.Access_Token);
                if (githubUser == null)
                {
                    _logger.LogWarning("Failed to get GitHub user info");
                    return new AuthResponse { Success = false, Error = "Failed to get GitHub user info" };
                }

                // Find or create user
                var user = await FindOrCreateUserAsync(githubUser, tokenResponse);

                // Generate JWT token
                var jwtToken = await GenerateJwtTokenAsync(user);
                var refreshToken = await GenerateRefreshTokenAsync();

                // Update user's refresh token
                user.GitHubRefreshToken = refreshToken;
                user.TokenExpiresAt = DateTime.UtcNow.AddDays(30); // Refresh token expires in 30 days
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Successfully authenticated user {UserId} with GitHub", user.Id);

                return new AuthResponse
                {
                    Success = true,
                    Token = jwtToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1), // JWT expires in 1 hour
                    User = await MapToUserProfileAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GitHub OAuth authentication");
                return new AuthResponse { Success = false, Error = "Authentication failed" };
            }
        }

        /// <summary>
        /// Refresh JWT token using refresh token
        /// </summary>
        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.GitHubRefreshToken == refreshToken);

                if (user == null || user.TokenExpiresAt < DateTime.UtcNow)
                {
                    return new AuthResponse { Success = false, Error = "Invalid or expired refresh token" };
                }

                var jwtToken = await GenerateJwtTokenAsync(user);
                var newRefreshToken = await GenerateRefreshTokenAsync();

                user.GitHubRefreshToken = newRefreshToken;
                user.TokenExpiresAt = DateTime.UtcNow.AddDays(30);
                await _userManager.UpdateAsync(user);

                return new AuthResponse
                {
                    Success = true,
                    Token = jwtToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = await MapToUserProfileAsync(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new AuthResponse { Success = false, Error = "Token refresh failed" };
            }
        }

        /// <summary>
        /// Get user profile by user ID
        /// </summary>
        public async Task<UserProfile?> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? await MapToUserProfileAsync(user) : null;
        }

        /// <summary>
        /// Get user profile by GitHub ID
        /// </summary>
        public async Task<UserProfile?> GetUserProfileByGitHubIdAsync(long githubId)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.GitHubId == githubId);
            return user != null ? await MapToUserProfileAsync(user) : null;
        }

        /// <summary>
        /// Revoke user token
        /// </summary>
        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.GitHubRefreshToken == token);

                if (user != null)
                {
                    user.GitHubRefreshToken = null;
                    user.TokenExpiresAt = null;
                    await _userManager.UpdateAsync(user);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return false;
            }
        }

        /// <summary>
        /// Generate JWT token for user
        /// </summary>
        public async Task<string> GenerateJwtTokenAsync(MarketplaceUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "your-secret-key-here");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim("github_id", user.GitHubId?.ToString() ?? ""),
                    new Claim("github_username", user.GitHubUsername ?? "")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate refresh token
        /// </summary>
        public async Task<string> GenerateRefreshTokenAsync()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Get GitHub user info using access token
        /// </summary>
        public async Task<GitHubUserInfo?> GetGitHubUserInfoAsync(string accessToken)
        {
            try
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("Access token is null or empty");
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "PolyBucket.Marketplace.Api");

                var response = await _httpClient.GetAsync("https://api.github.com/user");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully retrieved GitHub user info");
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var userInfo = JsonSerializer.Deserialize<GitHubUserInfo>(content, options);
                    return userInfo;
                }

                _logger.LogWarning("Failed to get GitHub user info: {StatusCode}, Response: {Response}", response.StatusCode, content);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GitHub user info");
                return null;
            }
        }

        /// <summary>
        /// Exchange OAuth code for access token
        /// </summary>
        public async Task<GitHubTokenResponse?> ExchangeCodeForTokenAsync(string code)
        {
            try
            {
                var clientId = _configuration["GitHub:ClientId"];
                var clientSecret = _configuration["GitHub:ClientSecret"];
                var redirectUri = _configuration["GitHub:RedirectUri"];

                if (string.IsNullOrEmpty(clientId))
                {
                    _logger.LogError("GitHub ClientId is not configured");
                    return null;
                }

                if (string.IsNullOrEmpty(clientSecret))
                {
                    _logger.LogError("GitHub ClientSecret is not configured");
                    return null;
                }

                if (string.IsNullOrEmpty(redirectUri))
                {
                    _logger.LogError("GitHub RedirectUri is not configured");
                    return null;
                }

                _logger.LogInformation("Exchanging code for token. RedirectUri: {RedirectUri}, ClientId: {ClientId}", redirectUri, clientId);

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var requestBody = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri)
                });

                var response = await _httpClient.PostAsync("https://github.com/login/oauth/access_token", requestBody);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully exchanged code for token. Response length: {Length}", content.Length);
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var tokenResponse = JsonSerializer.Deserialize<GitHubTokenResponse>(content, options);
                    
                    if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Access_Token))
                    {
                        _logger.LogError("Token response is null or access token is empty. Response: {Response}", content);
                        return null;
                    }
                    
                    _logger.LogInformation("Token exchange successful. Token type: {TokenType}, Scope: {Scope}", 
                        tokenResponse.Token_Type, tokenResponse.Scope);
                    return tokenResponse;
                }

                _logger.LogWarning("Failed to exchange code for token: {StatusCode}, Response: {Response}", response.StatusCode, content);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging code for token");
                return null;
            }
        }

        /// <summary>
        /// Find existing user or create new one
        /// </summary>
        private async Task<MarketplaceUser> FindOrCreateUserAsync(GitHubUserInfo githubUser, GitHubTokenResponse tokenResponse)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.GitHubId == githubUser.Id);

            if (user == null)
            {
                // Create new user
                user = new MarketplaceUser
                {
                    UserName = githubUser.Login,
                    Email = githubUser.Email,
                    GitHubId = githubUser.Id,
                    GitHubUsername = githubUser.Login,
                    GitHubDisplayName = githubUser.Name,
                    GitHubAvatarUrl = githubUser.Avatar_Url,
                    GitHubProfileUrl = githubUser.Html_Url,
                    GitHubAccessToken = tokenResponse.Access_Token,
                    Bio = githubUser.Bio,
                    Location = githubUser.Location,
                    Website = githubUser.Blog,
                    Company = githubUser.Company,
                    IsVerified = githubUser.Site_Admin,
                    LastLoginAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                _logger.LogInformation("Created new user {UserId} for GitHub user {GitHubId}", user.Id, githubUser.Id);
            }
            else
            {
                // Update existing user
                user.Email = githubUser.Email;
                user.GitHubDisplayName = githubUser.Name;
                user.GitHubAvatarUrl = githubUser.Avatar_Url;
                user.GitHubProfileUrl = githubUser.Html_Url;
                user.GitHubAccessToken = tokenResponse.Access_Token;
                user.Bio = githubUser.Bio;
                user.Location = githubUser.Location;
                user.Website = githubUser.Blog;
                user.Company = githubUser.Company;
                user.IsVerified = githubUser.Site_Admin;
                user.LastLoginAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Updated existing user {UserId} for GitHub user {GitHubId}", user.Id, githubUser.Id);
            }

            return user;
        }

        /// <summary>
        /// Map user to user profile
        /// </summary>
        private async Task<UserProfile> MapToUserProfileAsync(MarketplaceUser user)
        {
            var pluginCount = await _context.Plugins.CountAsync(p => p.AuthorId == user.Id);
            var installationCount = await _context.PluginInstallations.CountAsync(i => i.UserId == user.Id);

            return new UserProfile
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                DisplayName = user.GitHubDisplayName,
                AvatarUrl = user.GitHubAvatarUrl,
                Bio = user.Bio,
                Location = user.Location,
                Website = user.Website,
                Company = user.Company,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                GitHubId = user.GitHubId,
                GitHubUsername = user.GitHubUsername,
                GitHubProfileUrl = user.GitHubProfileUrl,
                PluginCount = pluginCount,
                InstallationCount = installationCount
            };
        }
    }
}
