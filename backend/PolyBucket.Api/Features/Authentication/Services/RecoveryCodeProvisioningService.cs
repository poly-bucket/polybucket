using Microsoft.Extensions.Logging;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Users;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.Services;

public sealed class RecoveryCodeProvisioningService(
    PolyBucketDbContext context,
    IRecoveryCodeService recoveryCodeService,
    IRecoveryCodeRepository recoveryCodeRepository,
    TimeProvider timeProvider,
    ILogger<RecoveryCodeProvisioningService> logger) : IRecoveryCodeProvisioningService
{
    public async Task<IReadOnlyList<string>> ProvisionForNewUserAsync(
        User user,
        Guid actorUserId,
        bool storePendingDisplayPayload,
        UserAuditAction auditAction,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var batch = recoveryCodeService.CreateBatchEntities(user.Id, actorUserId, now);

        await recoveryCodeRepository.DeleteAllForUserAsync(user.Id, cancellationToken);
        await recoveryCodeRepository.AddCodesAsync(batch.Entities, cancellationToken);

        user.RecoveryCodesGeneratedAt = now;
        user.RecoveryCodesAcknowledgedAt = null;
        user.ProtectedPendingRecoveryCodesPayload = storePendingDisplayPayload
            ? recoveryCodeService.SerializeDisplayPayload(batch.PlaintextCodes)
            : null;
        user.UpdatedAt = now;

        context.UserAuditLogs.Add(new UserAuditLog
        {
            UserId = user.Id,
            PerformedByUserId = actorUserId,
            Action = auditAction,
            Details = $"Generated {batch.PlaintextCodes.Count} account recovery codes",
            CreatedAt = now
        });

        await recoveryCodeRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Provisioned {Count} recovery codes for user {UserId}", batch.PlaintextCodes.Count, user.Id);

        return batch.PlaintextCodes;
    }
}
