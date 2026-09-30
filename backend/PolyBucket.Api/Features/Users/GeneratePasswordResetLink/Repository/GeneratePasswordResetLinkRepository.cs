using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Users.Domain;

namespace PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Repository;

public class GeneratePasswordResetLinkRepository(PolyBucketDbContext context) : IGeneratePasswordResetLinkRepository
{
    public Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public void AddPasswordResetToken(PasswordResetToken token)
    {
        context.PasswordResetTokens.Add(token);
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
