using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.AcknowledgeRecoveryCodes.Repository;

public interface IAcknowledgeRecoveryCodesRepository
{
    Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);

    void AddAuditLog(UserAuditLog auditLog);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
