using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Services;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.Domain;

public class TestEmailConfigurationCommandHandler
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TestEmailConfigurationCommandHandler> _logger;

    public TestEmailConfigurationCommandHandler(
        IEmailService emailService,
        ILogger<TestEmailConfigurationCommandHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<TestEmailConfigurationResponse> Handle(TestEmailConfigurationCommand command)
    {
        try
        {
            EmailSettings emailSettings;
            
            if (command.EmailSettings != null)
            {
                // Use provided settings for testing
                emailSettings = command.EmailSettings;
            }
            else
            {
                // Use current saved settings
                emailSettings = await _emailService.GetEmailSettingsAsync();
            }

            var success = await _emailService.TestEmailConfigurationAsync(emailSettings, command.TestEmailAddress);

            return new TestEmailConfigurationResponse
            {
                Success = success,
                Message = success 
                    ? "Test email sent successfully! Please check your inbox." 
                    : "Failed to send test email. Please check your email configuration."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test email configuration");
            return new TestEmailConfigurationResponse
            {
                Success = false,
                Message = "An error occurred while testing email configuration"
            };
        }
    }
}

public class TestEmailConfigurationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
} 