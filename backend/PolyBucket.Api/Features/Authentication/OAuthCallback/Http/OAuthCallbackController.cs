using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.OAuthCallback.Domain;
using PolyBucket.Api.Features.Authentication.Services;

namespace PolyBucket.Api.Features.Authentication.OAuthCallback.Http;

[ApiController]
[Route("api/auth/oauth")]
public class OAuthCallbackController(
    IOAuthService oauthService,
    IConfiguration configuration,
    ILogger<OAuthCallbackController> logger) : ControllerBase
{
    /// <summary>
    /// Complete OAuth sign-in with an authorization code from the provider.
    /// </summary>
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
            logger.LogWarning("OAuth error from {Provider}: {Error} - {Description}", provider, command.Error, command.ErrorDescription);
            return BadRequest(new { message = command.ErrorDescription ?? "OAuth authentication failed" });
        }

        try
        {
            var redirectUri = $"{configuration["AppSettings:Frontend:BaseUrl"]}/auth/callback";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            var response = await oauthService.AuthenticateWithOAuthAsync(provider, command.Code, redirectUri, ipAddress);

            return Ok(new OAuthCallbackCommandResponse
            {
                Authentication = OAuthCallbackAuthenticationMapper.From(response),
                IsNewUser = false,
                Provider = provider
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "OAuth authentication failed for provider {Provider}", provider);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during OAuth callback for provider {Provider}", provider);
            return StatusCode(500, new { message = "An unexpected error occurred during OAuth authentication" });
        }
    }
}
