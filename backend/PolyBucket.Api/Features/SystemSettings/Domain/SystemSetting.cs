using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.SystemSettings.Domain;

public class SystemSetting
{
    [Key]
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}

public static class SystemSettingKeys
{
    public const string AdminSetupCompleted = "Setup:AdminCompleted";
    public const string RoleSetupCompleted = "Setup:RolesCompleted";
    public const string ModerationSetupCompleted = "Setup:ModerationCompleted";
} 