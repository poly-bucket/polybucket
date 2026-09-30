using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Users.Domain;

namespace PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Repository;

public interface IGeneratePasswordResetLinkRepository
{
    Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void AddPasswordResetToken(PasswordResetToken token);
    void AddAuditLog(UserAuditLog auditLog);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
