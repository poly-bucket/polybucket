using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Api.Features.SystemSettings.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.Http;

[ApiController]
[Route("api/system-settings/email")]
[Authorize(Roles = "Admin")]
public class GetEmailSettingsController(
    IEmailService emailService,
    ILogger<GetEmailSettingsController> logger) : ControllerBase
{
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<GetEmailSettingsController> _logger = logger;

    /// <summary>
    /// Get current email service configuration
    /// </summary>
    /// <returns>Current email settings</returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(EmailSettingsResponse))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetEmailSettings()
    {
        try
        {
            var emailSettings = await _emailService.GetEmailSettingsAsync();
            var isConfigured = await _emailService.IsEmailServiceConfiguredAsync();

            // Don't return the password in the response for security
            var response = new EmailSettingsResponse
            {
                Enabled = emailSettings.Enabled,
                SmtpServer = emailSettings.SmtpServer,
                SmtpPort = emailSettings.SmtpPort,
                SmtpUsername = emailSettings.SmtpUsername,
                HasPassword = !string.IsNullOrEmpty(emailSettings.SmtpPassword),
                UseSsl = emailSettings.UseSsl,
                FromAddress = emailSettings.FromAddress,
                FromName = emailSettings.FromName,
                RequireEmailVerification = emailSettings.RequireEmailVerification,
                IsConfigured = isConfigured
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email settings");
            return StatusCode(500, new { message = "An unexpected error occurred while retrieving email settings" });
        }
    }
}

public class EmailSettingsResponse
{
    public bool Enabled { get; set; }
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public bool HasPassword { get; set; }
    public bool UseSsl { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool RequireEmailVerification { get; set; }
    public bool IsConfigured { get; set; }
} 