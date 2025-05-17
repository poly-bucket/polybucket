namespace Api.Controllers.UserSettings.Domain;

public class UpdateUserSettingsRequest
{
    public Guid UserId { get; set; }
    public string Language { get; set; }
    public string Theme { get; set; }
    public bool? EmailNotifications { get; set; }
    public Guid? DefaultPrinterId { get; set; }
    public string MeasurementSystem { get; set; }
    public string TimeZone { get; set; }
    public Dictionary<string, string> CustomSettings { get; set; }
}