using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Users.Domain;

namespace PolyBucket.Api.Features.Users.MarkEmailVerified.Repository;

public interface IMarkEmailVerifiedRepository
{
    Task<User?> FindByIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default);
    void AddAuditLog(UserAuditLog auditLog);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
