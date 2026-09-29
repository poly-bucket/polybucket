using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Domain;

public interface IModerationAuditLogWriter
{
    Task WriteAsync(
        Guid modelId,
        Guid performedByUserId,
        ModerationAction action,
        string? previousValues,
        string? newValues,
        string? moderationNotes,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);
}
