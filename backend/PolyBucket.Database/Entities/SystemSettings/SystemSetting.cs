using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Database.Entities.SystemSettings;

public class SystemSetting
{
    [Key]
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}
