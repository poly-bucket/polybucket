namespace PolyBucket.Storage;

public class StorageSettingsDto
{
    public string Provider { get; set; } = "minio";
    public string Endpoint { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string BucketName { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public bool HasSecretKey { get; set; }
    public string? ExternalEndpoint { get; set; }
    public int? ExternalPort { get; set; }
    public bool? ExternalUseSsl { get; set; }
    public string Region { get; set; } = "us-east-1";
    public bool IsConfigured { get; set; }
    public DateTime? LastSuccessfulTestAt { get; set; }
    public List<string> ValidationErrors { get; set; } = [];
    public Dictionary<string, StorageSettingSource> Sources { get; set; } = [];
    public List<string> ManagedByEnvironment { get; set; } = [];

    public static StorageSettingsDto From(EffectiveStorageSettings settings)
    {
        return new StorageSettingsDto
        {
            Provider = settings.Provider,
            Endpoint = settings.Endpoint,
            Port = settings.Port,
            UseSsl = settings.UseSsl,
            BucketName = settings.BucketName,
            AccessKey = settings.AccessKey,
            HasSecretKey = !string.IsNullOrEmpty(settings.SecretKey),
            ExternalEndpoint = settings.ExternalEndpoint,
            ExternalPort = settings.ExternalPort,
            ExternalUseSsl = settings.ExternalUseSsl,
            Region = settings.Region,
            IsConfigured = settings.IsConfigured,
            LastSuccessfulTestAt = settings.LastSuccessfulTestAt,
            ValidationErrors = [.. settings.GetValidationErrors()],
            Sources = settings.Sources.ToDictionary(pair => pair.Key, pair => pair.Value),
            ManagedByEnvironment =
            [
                .. StorageSettingsFields.AllNames.Where(settings.IsManagedByEnvironment)
            ]
        };
    }
}

public static class StorageSettingsFields
{
    public static readonly string[] AllNames =
    [
        EffectiveStorageSettings.Fields.Provider,
        EffectiveStorageSettings.Fields.Endpoint,
        EffectiveStorageSettings.Fields.Port,
        EffectiveStorageSettings.Fields.UseSsl,
        EffectiveStorageSettings.Fields.BucketName,
        EffectiveStorageSettings.Fields.AccessKey,
        EffectiveStorageSettings.Fields.SecretKey,
        EffectiveStorageSettings.Fields.ExternalEndpoint,
        EffectiveStorageSettings.Fields.ExternalPort,
        EffectiveStorageSettings.Fields.ExternalUseSsl,
        EffectiveStorageSettings.Fields.Region
    ];
}
