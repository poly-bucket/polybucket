using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Http;

public class UpdateStorageSettingsRequest
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 9000;

    public bool UseSsl { get; set; }

    [Required]
    public string BucketName { get; set; } = string.Empty;

    [Required]
    public string AccessKey { get; set; } = string.Empty;

    public string? SecretKey { get; set; }

    public bool ClearSecretKey { get; set; }

    public string? ExternalEndpoint { get; set; }

    [Range(1, 65535)]
    public int? ExternalPort { get; set; }

    public bool? ExternalUseSsl { get; set; }

    public string Region { get; set; } = "us-east-1";

    public bool ForcePathStyle { get; set; }
}
