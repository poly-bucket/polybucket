using System;
using PolyBucket.Api.Common.Email.Templates;

namespace PolyBucket.Api.Features.Email.Domain;

public class EmailMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public EmailTemplateKey TemplateKey { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string ModelJson { get; set; } = "{}";
    public EmailMessageStatus Status { get; set; } = EmailMessageStatus.Pending;
    public int Attempts { get; set; }
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;
    public DateTime? LockedUntil { get; set; }
    public string? LastError { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
}

public enum EmailMessageStatus
{
    Pending,
    Sending,
    Sent,
    Failed,
    DeadLetter
}
