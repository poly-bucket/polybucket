using PolyBucket.Storage.Settings;

namespace PolyBucket.Api.Settings;

public class AppSettings
{
    public SecuritySettings Security { get; set; } = new();
    public StorageSettings Storage { get; set; } = new();
    public FrontendSettings Frontend { get; set; } = new();
}

public class FrontendSettings
{
    public string BaseUrl { get; set; } = string.Empty;
}
