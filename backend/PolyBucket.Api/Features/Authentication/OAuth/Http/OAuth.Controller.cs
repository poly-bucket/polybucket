using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.OAuth.Domain;
using PolyBucket.Api.Features.Authentication.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.OAuth.Http
{
    [ApiController]
    [Route("api/auth/oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly IOAuthService _oauthService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OAuthController> _logger;

        public OAuthController(
            IOAuthService oauthService,
            IConfiguration configuration,
            ILogger<OAuthController> logger)
        {
            _oauthService = oauthService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("{provider}/authorize")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetAuthorizationUrl(string provider, [FromQuery] string redirectUri, [FromQuery] string? state = null)
        {
            try
            {
                var stateParam = state ?? Guid.NewGuid().ToString();
                var authUrl = await _oauthService.GetAuthorizationUrlAsync(provider, redirectUri, stateParam);
                return Ok(new { authorizationUrl = authUrl, state = stateParam });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authorization URL for provider {Provider}", provider);
                return BadRequest(new { message = "Invalid OAuth provider or configuration" });
            }
        }

        [HttpPost("{provider}/callback")]
        [ProducesResponseType(200, Type = typeof(OAuthCallbackCommandResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> OAuthCallback(string provider, [FromBody] OAuthCallbackCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (command.Provider != provider)
            {
                return BadRequest(new { message = "Provider mismatch" });
            }

            if (!string.IsNullOrEmpty(command.Error))
            {
                _logger.LogWarning("OAuth error from {Provider}: {Error} - {Description}", provider, command.Error, command.ErrorDescription);
                return BadRequest(new { message = command.ErrorDescription ?? "OAuth authentication failed" });
            }

            try
            {
                var redirectUri = $"{_configuration["AppSettings:Frontend:BaseUrl"]}/auth/callback";
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                
                var response = await _oauthService.AuthenticateWithOAuthAsync(provider, command.Code, redirectUri, ipAddress);
                
                return Ok(new OAuthCallbackCommandResponse
                {
                    Authentication = response,
                    IsNewUser = false, // TODO: Determine if this is a new user
                    Provider = provider
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "OAuth authentication failed for provider {Provider}", provider);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OAuth callback for provider {Provider}", provider);
                return StatusCode(500, new { message = "An unexpected error occurred during OAuth authentication" });
            }
        }
    }
} 