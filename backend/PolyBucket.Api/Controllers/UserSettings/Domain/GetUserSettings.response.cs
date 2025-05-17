using Core.Models.Users.Settings;

namespace Api.Controllers.UserSettings.Domain;

public class GetUserSettingsResponse
{
    public Core.Models.Users.Settings.UserSettings Settings { get; set; }
}