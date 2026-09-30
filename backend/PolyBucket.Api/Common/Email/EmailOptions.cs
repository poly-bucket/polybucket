namespace PolyBucket.Api.Common.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public EmailTransportKind? Transport { get; set; }
    public string? FromAddress { get; set; }
    public string? FromName { get; set; }
    public string? ReplyTo { get; set; }
    public string? PublicBaseUrl { get; set; }
    public bool? RequireEmailVerification { get; set; }
    public SmtpOptions Smtp { get; set; } = new();
    public EmailDispatcherOptions Dispatcher { get; set; } = new();
}

public class SmtpOptions
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    public EmailSecurityMode? Security { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PasswordFile { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public bool? AllowInvalidCertificates { get; set; }
}

public class EmailDispatcherOptions
{
    public bool Enabled { get; set; } = true;
    public int BatchSize { get; set; } = 20;
    public int MaxAttempts { get; set; } = 8;
    public int PollSeconds { get; set; } = 5;
    public int LockSeconds { get; set; } = 120;
    public int SentRetentionDays { get; set; } = 30;
}
