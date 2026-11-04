using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;

using PolyBucket.Database.Entities.Users;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.AcknowledgeRecoveryCodes.Repository;

public sealed class AcknowledgeRecoveryCodesRepository(PolyBucketDbContext context) : IAcknowledgeRecoveryCodesRepository
{
    public Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public void AddAuditLog(UserAuditLog auditLog) => context.UserAuditLogs.Add(auditLog);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
