using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Common.Email;

public sealed record EffectiveEmailSettings
{
    public static class Fields
    {
        public const string Transport = "transport";
        public const string SmtpHost = "smtpHost";
        public const string SmtpPort = "smtpPort";
        public const string SmtpSecurity = "smtpSecurity";
        public const string SmtpUsername = "smtpUsername";
        public const string SmtpPassword = "smtpPassword";
        public const string AllowInvalidCertificates = "allowInvalidCertificates";
        public const string FromAddress = "fromAddress";
        public const string FromName = "fromName";
        public const string ReplyTo = "replyTo";
        public const string PublicBaseUrl = "publicBaseUrl";
        public const string RequireEmailVerification = "requireEmailVerification";

        public static readonly IReadOnlyList<string> All =
        [
            Transport, SmtpHost, SmtpPort, SmtpSecurity, SmtpUsername, SmtpPassword, AllowInvalidCertificates,
            FromAddress, FromName, ReplyTo, PublicBaseUrl, RequireEmailVerification
        ];
    }

    public EmailTransportKind Transport { get; init; } = EmailTransportKind.Disabled;
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; } = 587;
    public EmailSecurityMode SmtpSecurity { get; init; } = EmailSecurityMode.Auto;
    public string SmtpUsername { get; init; } = string.Empty;
    public string SmtpPassword { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 30;
    public bool AllowInvalidCertificates { get; init; }
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = "PolyBucket";
    public string? ReplyTo { get; init; }
    public string PublicBaseUrl { get; init; } = string.Empty;
    public bool RequireEmailVerification { get; init; }
    public DateTime? LastSuccessfulTestAt { get; init; }
    public IReadOnlyDictionary<string, EmailSettingSource> Sources { get; init; } = new Dictionary<string, EmailSettingSource>();

    public bool IsEnabled => Transport != EmailTransportKind.Disabled;

    public bool IsManagedByEnvironment(string field) =>
        Sources.TryGetValue(field, out var source) && source == EmailSettingSource.Environment;

    public IReadOnlyList<string> GetValidationErrors()
    {
        var errors = new List<string>();
        if (!IsEnabled)
        {
            return errors;
        }

        if (string.IsNullOrWhiteSpace(FromAddress) || !EmailAddressValidator.IsValid(FromAddress))
        {
            errors.Add("A valid From address is required.");
        }

        if (Transport == EmailTransportKind.Smtp)
        {
            if (string.IsNullOrWhiteSpace(SmtpHost))
            {
                errors.Add("SMTP host is required.");
            }

            if (SmtpPort is < 1 or > 65535)
            {
                errors.Add("SMTP port must be between 1 and 65535.");
            }
        }

        if (!string.IsNullOrWhiteSpace(ReplyTo) && !EmailAddressValidator.IsValid(ReplyTo))
        {
            errors.Add("Reply-To must be a valid email address.");
        }

        if (!string.IsNullOrWhiteSpace(PublicBaseUrl) && !Uri.TryCreate(PublicBaseUrl, UriKind.Absolute, out _))
        {
            errors.Add("Public base URL must be an absolute URL.");
        }

        return errors;
    }

    public bool IsValid => GetValidationErrors().Count == 0;

    public bool CanDeliver => IsEnabled && IsValid;

    public bool HasPublicBaseUrl => Uri.TryCreate(PublicBaseUrl, UriKind.Absolute, out _);

    public string BuildUrl(string relativePath)
    {
        return PublicBaseUrl.TrimEnd('/') + "/" + relativePath.TrimStart('/');
    }
}

public static class EmailAddressValidator
{
    public static bool IsValid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && MimeKit.MailboxAddress.TryParse(value, out var mailbox)
            && mailbox.Address.Contains('@');
    }
}
