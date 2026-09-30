using System;
using System.Collections.Generic;
using System.Linq;
using PolyBucket.Api.Common.Email;

namespace PolyBucket.Api.Features.Email.Domain;

public class EmailSettingsDto
{
    public bool Enabled { get; set; }
    public EmailTransportKind Transport { get; set; }
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public EmailSecurityMode SmtpSecurity { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public bool HasPassword { get; set; }
    public bool AllowInvalidCertificates { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string? ReplyTo { get; set; }
    public string PublicBaseUrl { get; set; } = string.Empty;
    public bool RequireEmailVerification { get; set; }
    public bool IsConfigured { get; set; }
    public DateTime? LastSuccessfulTestAt { get; set; }
    public List<string> ValidationErrors { get; set; } = [];
    public Dictionary<string, EmailSettingSource> Sources { get; set; } = [];
    public List<string> ManagedByEnvironment { get; set; } = [];

    public static EmailSettingsDto From(EffectiveEmailSettings settings)
    {
        return new EmailSettingsDto
        {
            Enabled = settings.IsEnabled,
            Transport = settings.Transport,
            SmtpHost = settings.SmtpHost,
            SmtpPort = settings.SmtpPort,
            SmtpSecurity = settings.SmtpSecurity,
            SmtpUsername = settings.SmtpUsername,
            HasPassword = !string.IsNullOrEmpty(settings.SmtpPassword),
            AllowInvalidCertificates = settings.AllowInvalidCertificates,
            FromAddress = settings.FromAddress,
            FromName = settings.FromName,
            ReplyTo = settings.ReplyTo,
            PublicBaseUrl = settings.PublicBaseUrl,
            RequireEmailVerification = settings.RequireEmailVerification,
            IsConfigured = settings.CanDeliver,
            LastSuccessfulTestAt = settings.LastSuccessfulTestAt,
            ValidationErrors = [.. settings.GetValidationErrors()],
            Sources = settings.Sources.ToDictionary(pair => pair.Key, pair => pair.Value),
            ManagedByEnvironment = [.. EffectiveEmailSettings.Fields.All.Where(settings.IsManagedByEnvironment)]
        };
    }
}
