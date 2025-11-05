using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Marketplace.Api.Models;
using PolyBucket.Marketplace.Api.Services;
using System.Security.Claims;

namespace PolyBucket.Marketplace.Api.Controllers
{
    /// <summary>
    /// Controller for handling authentication and user management
    /// </summary>
    /// <remarks>
    /// This controller provides authentication endpoints using GitHub OAuth, JWT token management,
    /// and user profile retrieval. It supports the complete authentication flow including
    /// OAuth callbacks, token refresh, and user profile management.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IConfiguration configuration)
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get GitHub OAuth authorization URL
        /// </summary>
        /// <returns>GitHub OAuth URL</returns>
        [HttpGet("github/url")]
        public IActionResult GetGitHubAuthUrl()
        {
            try
            {
                var clientId = _configuration["GitHub:ClientId"] ?? 
                               Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID") ?? string.Empty;
                var redirectUri = _configuration["GitHub:RedirectUri"] ?? 
                                  Environment.GetEnvironmentVariable("GITHUB_REDIRECT_URI") ?? 
                                  "http://localhost:10110/auth/callback";
                var state = Guid.NewGuid().ToString();

                var authUrl = $"https://github.com/login/oauth/authorize?" +
                    $"client_id={clientId}&" +
                    $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                    $"scope=user:email,read:user&" +
                    $"state={state}";

                return Ok(new { authUrl, state });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating GitHub auth URL");
                return StatusCode(500, new { error = "Failed to generate auth URL" });
            }
        }

        /// <summary>
        /// Handle GitHub OAuth callback
        /// </summary>
        /// <param name="callback">OAuth callback data</param>
        /// <returns>Authentication response</returns>
        [HttpPost("github/callback")]
        public async Task<IActionResult> GitHubCallback([FromBody] GitHubOAuthCallback callback)
        {
            try
            {
                if (string.IsNullOrEmpty(callback.Code))
                {
                    return BadRequest(new { error = "Authorization code is required" });
                }

                var result = await _authService.AuthenticateWithGitHubAsync(callback.Code, callback.State);
                
                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GitHub OAuth callback");
                return StatusCode(500, new { error = "Authentication failed" });
            }
        }

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>New authentication response</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new { error = "Refresh token is required" });
                }

                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                
                if (!result.Success)
                {
                    return Unauthorized(new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { error = "Token refresh failed" });
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <returns>User profile</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var profile = await _authService.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    return NotFound(new { error = "User profile not found" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { error = "Failed to get user profile" });
            }
        }

        /// <summary>
        /// Get user profile by GitHub ID
        /// </summary>
        /// <param name="githubId">GitHub user ID</param>
        /// <returns>User profile</returns>
        [HttpGet("profile/github/{githubId}")]
        public async Task<IActionResult> GetProfileByGitHubId(long githubId)
        {
            try
            {
                var profile = await _authService.GetUserProfileByGitHubIdAsync(githubId);
                if (profile == null)
                {
                    return NotFound(new { error = "User profile not found" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile by GitHub ID");
                return StatusCode(500, new { error = "Failed to get user profile" });
            }
        }

        /// <summary>
        /// Logout user (revoke refresh token)
        /// </summary>
        /// <param name="refreshToken">Refresh token to revoke</param>
        /// <returns>Logout result</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new { error = "Refresh token is required" });
                }

                var success = await _authService.RevokeTokenAsync(request.RefreshToken);
                
                if (!success)
                {
                    return StatusCode(500, new { error = "Failed to logout" });
                }

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { error = "Logout failed" });
            }
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <returns>Token validation result</returns>
        [HttpGet("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var githubId = User.FindFirst("github_id")?.Value;
                var githubUsername = User.FindFirst("github_username")?.Value;

                return Ok(new
                {
                    valid = true,
                    userId,
                    username,
                    email,
                    githubId,
                    githubUsername
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return Unauthorized(new { error = "Invalid token" });
            }
        }
    }

    /// <summary>
    /// Request model for refresh token
    /// </summary>
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for logout
    /// </summary>
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
