using System.ComponentModel.DataAnnotations;
using PolyBucket.Api.Common.Email;

namespace PolyBucket.Api.Features.Email.UpdateEmailSettings.Http;

public class UpdateEmailSettingsRequest
{
    public EmailTransportKind Transport { get; set; } = EmailTransportKind.Disabled;

    [MaxLength(255)]
    public string SmtpHost { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int SmtpPort { get; set; } = 587;

    public EmailSecurityMode SmtpSecurity { get; set; } = EmailSecurityMode.Auto;

    [MaxLength(255)]
    public string SmtpUsername { get; set; } = string.Empty;

    public string? SmtpPassword { get; set; }

    public bool ClearPassword { get; set; }

    public bool AllowInvalidCertificates { get; set; }

    [MaxLength(320)]
    public string FromAddress { get; set; } = string.Empty;

    [MaxLength(200)]
    public string FromName { get; set; } = "PolyBucket";

    [MaxLength(320)]
    public string? ReplyTo { get; set; }

    [MaxLength(2048)]
    public string PublicBaseUrl { get; set; } = string.Empty;

    public bool RequireEmailVerification { get; set; }
}
