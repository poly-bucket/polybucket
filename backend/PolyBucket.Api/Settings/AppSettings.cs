namespace PolyBucket.Api.Settings;

public class AppSettings
{
    public SecuritySettings Security { get; set; } = new();
    public StorageSettings Storage { get; set; } = new();
} 