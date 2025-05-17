using Core.Entities;
using Core.Enumerations;

namespace Core.Models.Users.Settings;

public class UserSettings : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string Language { get; set; } = "en";
    public string Theme { get; set; } = "dark";
    public bool EmailNotifications { get; set; } = true;
    public Guid? DefaultPrinterId { get; set; }
    public string MeasurementSystem { get; set; } = "metric";
    public string TimeZone { get; set; } = "UTC";
    public Dictionary<string, string> CustomSettings { get; set; } = new();
} 