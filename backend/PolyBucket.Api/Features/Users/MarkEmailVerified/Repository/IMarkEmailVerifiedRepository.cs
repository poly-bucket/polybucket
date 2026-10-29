using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Users;

namespace PolyBucket.Api.Features.Users.MarkEmailVerified.Repository;

public interface IMarkEmailVerifiedRepository
{
    Task<User?> FindByIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default);
    void AddAuditLog(UserAuditLog auditLog);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
