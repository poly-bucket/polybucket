using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.Domain;

namespace PolyBucket.Api.Features.Users.MarkEmailVerified.Repository;

public class MarkEmailVerifiedRepository(PolyBucketDbContext context) : IMarkEmailVerifiedRepository
{
    public Task<User?> FindByIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public void AddAuditLog(UserAuditLog auditLog)
    {
        context.UserAuditLogs.Add(auditLog);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
