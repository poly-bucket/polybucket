using System;

namespace PolyBucket.Api.Features.Users.Domain;

public class UserAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public UserAuditAction Action { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum UserAuditAction
{
    EmailMarkedVerified,
    PasswordResetLinkGenerated
}
