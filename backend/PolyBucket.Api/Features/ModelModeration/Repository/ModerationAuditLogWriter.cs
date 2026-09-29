using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Repository;

public class ModerationAuditLogWriter(PolyBucketDbContext context) : IModerationAuditLogWriter
{
    private readonly PolyBucketDbContext _context = context;

    public async Task WriteAsync(
        Guid modelId,
        Guid performedByUserId,
        ModerationAction action,
        string? previousValues,
        string? newValues,
        string? moderationNotes,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        _context.ModerationAuditLogs.Add(new ModerationAuditLog
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            PerformedByUserId = performedByUserId,
            Action = action,
            PreviousValues = previousValues,
            NewValues = newValues,
            ModerationNotes = moderationNotes,
            IPAddress = ipAddress,
            UserAgent = userAgent,
            PerformedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
