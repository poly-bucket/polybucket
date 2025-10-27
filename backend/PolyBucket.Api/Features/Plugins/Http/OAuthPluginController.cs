using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Plugins.Http
{
    [ApiController]
    [Route("api/plugins/oauth")]
    public class OAuthPluginController : ControllerBase
    {
        private readonly OAuthPluginService _oauthService;
        private readonly ILogger<OAuthPluginController> _logger;

        public OAuthPluginController(
            OAuthPluginService oauthService,
            ILogger<OAuthPluginController> logger)
        {
            _oauthService = oauthService;
            _logger = logger;
        }

        /// <summary>
        /// Get all registered OAuth providers
        /// </summary>
        /// <returns>List of OAuth providers</returns>
        [HttpGet("providers")]
        [ProducesResponseType(typeof(List<OAuthProviderInfo>), 200)]
        [ProducesResponseType(500)]
        public ActionResult<List<OAuthProviderInfo>> GetOAuthProviders()
        {
            try
            {
                var providers = _oauthService.GetRegisteredProviders();
                return Ok(providers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OAuth providers");
                return StatusCode(500, new { message = "Error retrieving OAuth providers" });
            }
        }

        /// <summary>
        /// Get OAuth authorization URL for a provider
        /// </summary>
        /// <param name="providerName">OAuth provider name</param>
        /// <param name="redirectUri">Redirect URI after authorization</param>
        /// <param name="state">Optional state parameter</param>
        /// <returns>Authorization URL</returns>
        [HttpGet("authorize/{providerName}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public ActionResult GetAuthorizationUrl(string providerName, [FromQuery] string redirectUri, [FromQuery] string? state = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(providerName))
                {
                    return BadRequest(new { message = "Provider name is required" });
                }

                if (string.IsNullOrWhiteSpace(redirectUri))
                {
                    return BadRequest(new { message = "Redirect URI is required" });
                }

                if (!_oauthService.IsProviderRegistered(providerName))
                {
                    return NotFound(new { message = $"OAuth provider '{providerName}' is not registered" });
                }

                // TODO: Generate authorization URL using provider configuration
                var authUrl = $"https://example.com/oauth/authorize?client_id=example&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope=read&state={state ?? Guid.NewGuid().ToString()}";

                return Ok(new
                {
                    authorizationUrl = authUrl,
                    providerName,
                    redirectUri,
                    state
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authorization URL for provider {ProviderName}", providerName);
                return StatusCode(500, new { message = "Error generating authorization URL" });
            }
        }

        /// <summary>
        /// Handle OAuth callback and exchange code for tokens
        /// </summary>
        /// <param name="providerName">OAuth provider name</param>
        /// <param name="request">OAuth callback request</param>
        /// <returns>Authorization result</returns>
        [HttpPost("callback/{providerName}")]
        [ProducesResponseType(typeof(OAuthAuthorizationResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OAuthAuthorizationResult>> HandleCallback(
            string providerName, 
            [FromBody] OAuthCallbackRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(providerName))
                {
                    return BadRequest(new { message = "Provider name is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Code))
                {
                    return BadRequest(new { message = "Authorization code is required" });
                }

                if (string.IsNullOrWhiteSpace(request.RedirectUri))
                {
                    return BadRequest(new { message = "Redirect URI is required" });
                }

                if (!_oauthService.IsProviderRegistered(providerName))
                {
                    return NotFound(new { message = $"OAuth provider '{providerName}' is not registered" });
                }

                var result = await _oauthService.AuthorizeAsync(providerName, request.Code, request.RedirectUri);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling OAuth callback for provider {ProviderName}", providerName);
                return StatusCode(500, new { message = "Error processing OAuth callback" });
            }
        }

        /// <summary>
        /// Get user information from OAuth provider
        /// </summary>
        /// <param name="providerName">OAuth provider name</param>
        /// <param name="accessToken">Access token</param>
        /// <returns>User information</returns>
        [HttpGet("userinfo/{providerName}")]
        [ProducesResponseType(typeof(OAuthUserInfo), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OAuthUserInfo>> GetUserInfo(string providerName, [FromQuery] string accessToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(providerName))
                {
                    return BadRequest(new { message = "Provider name is required" });
                }

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return BadRequest(new { message = "Access token is required" });
                }

                if (!_oauthService.IsProviderRegistered(providerName))
                {
                    return NotFound(new { message = $"OAuth provider '{providerName}' is not registered" });
                }

                var userInfo = await _oauthService.GetUserInfoAsync(providerName, accessToken);
                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info from provider {ProviderName}", providerName);
                return StatusCode(500, new { message = "Error retrieving user information" });
            }
        }

        /// <summary>
        /// Refresh OAuth access token
        /// </summary>
        /// <param name="providerName">OAuth provider name</param>
        /// <param name="request">Token refresh request</param>
        /// <returns>Token refresh result</returns>
        [HttpPost("refresh/{providerName}")]
        [ProducesResponseType(typeof(OAuthTokenRefreshResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OAuthTokenRefreshResult>> RefreshToken(
            string providerName, 
            [FromBody] OAuthRefreshRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(providerName))
                {
                    return BadRequest(new { message = "Provider name is required" });
                }

                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return BadRequest(new { message = "Refresh token is required" });
                }

                if (!_oauthService.IsProviderRegistered(providerName))
                {
                    return NotFound(new { message = $"OAuth provider '{providerName}' is not registered" });
                }

                var result = await _oauthService.RefreshTokenAsync(providerName, request.RefreshToken);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token for provider {ProviderName}", providerName);
                return StatusCode(500, new { message = "Error refreshing token" });
            }
        }

        /// <summary>
        /// Validate OAuth access token
        /// </summary>
        /// <param name="providerName">OAuth provider name</param>
        /// <param name="accessToken">Access token to validate</param>
        /// <returns>Token validation result</returns>
        [HttpPost("validate/{providerName}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> ValidateToken(string providerName, [FromBody] OAuthValidateRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(providerName))
                {
                    return BadRequest(new { message = "Provider name is required" });
                }

                if (string.IsNullOrWhiteSpace(request.AccessToken))
                {
                    return BadRequest(new { message = "Access token is required" });
                }

                if (!_oauthService.IsProviderRegistered(providerName))
                {
                    return NotFound(new { message = $"OAuth provider '{providerName}' is not registered" });
                }

                var isValid = await _oauthService.ValidateTokenAsync(providerName, request.AccessToken);
                
                return Ok(new { valid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token for provider {ProviderName}", providerName);
                return StatusCode(500, new { message = "Error validating token" });
            }
        }
    }

    public class OAuthCallbackRequest
    {
        public string Code { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string? State { get; set; }
    }

    public class OAuthRefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class OAuthValidateRequest
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
