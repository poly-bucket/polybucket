using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.Authentication.RecoveryCodes.AcknowledgeRecoveryCodes.Repository;
using PolyBucket.Database.Entities.Users;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.AcknowledgeRecoveryCodes.Domain;

public class AcknowledgeRecoveryCodesCommandHandler(
    IHttpContextAccessor httpContextAccessor,
    IAcknowledgeRecoveryCodesRepository repository,
    TimeProvider timeProvider)
{
    public async Task Handle(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var user = await repository.GetUserAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException();

        var now = timeProvider.GetUtcNow().UtcDateTime;
        user.RecoveryCodesAcknowledgedAt = now;
        user.ProtectedPendingRecoveryCodesPayload = null;
        user.UpdatedAt = now;

        repository.AddAuditLog(new UserAuditLog
        {
            UserId = user.Id,
            PerformedByUserId = userId,
            Action = UserAuditAction.RecoveryCodesAcknowledged,
            Details = "Account recovery codes acknowledged by user",
            CreatedAt = now
        });

        await repository.SaveChangesAsync(cancellationToken);
    }

    private Guid GetUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        return userId;
    }
}
