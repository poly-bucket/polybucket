using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.Domain;

public class UpdateEmailSettingsCommandHandler
{
    private readonly PolyBucketDbContext _context;
    private readonly ILogger<UpdateEmailSettingsCommandHandler> _logger;

    public UpdateEmailSettingsCommandHandler(
        PolyBucketDbContext context,
        ILogger<UpdateEmailSettingsCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UpdateEmailSettingsResponse> Handle(UpdateEmailSettingsCommand command)
    {
        try
        {
            // Define the settings to update
            var settingsToUpdate = new Dictionary<string, string>
            {
                { SystemSettingKeys.EmailEnabled, command.Enabled.ToString().ToLower() },
                { SystemSettingKeys.EmailSmtpServer, command.SmtpServer },
                { SystemSettingKeys.EmailSmtpPort, command.SmtpPort.ToString() },
                { SystemSettingKeys.EmailSmtpUsername, command.SmtpUsername },
                { SystemSettingKeys.EmailSmtpPassword, command.SmtpPassword },
                { SystemSettingKeys.EmailUseSsl, command.UseSsl.ToString().ToLower() },
                { SystemSettingKeys.EmailFromAddress, command.FromAddress },
                { SystemSettingKeys.EmailFromName, command.FromName },
                { SystemSettingKeys.EmailRequireVerification, command.RequireEmailVerification.ToString().ToLower() }
            };

            // Get existing settings
            var existingSettings = await _context.SystemSettings
                .Where(s => settingsToUpdate.Keys.Contains(s.Key))
                .ToListAsync();

            // Update or create settings
            foreach (var settingPair in settingsToUpdate)
            {
                var existing = existingSettings.FirstOrDefault(s => s.Key == settingPair.Key);
                if (existing != null)
                {
                    existing.Value = settingPair.Value;
                }
                else
                {
                    _context.SystemSettings.Add(new SystemSetting
                    {
                        Key = settingPair.Key,
                        Value = settingPair.Value
                    });
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Email settings updated successfully");

            return new UpdateEmailSettingsResponse
            {
                Success = true,
                Message = "Email settings updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update email settings");
            return new UpdateEmailSettingsResponse
            {
                Success = false,
                Message = "Failed to update email settings"
            };
        }
    }
}

public class UpdateEmailSettingsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
} 