using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Database.Entities.Users;

namespace PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Repository;

public interface IGeneratePasswordResetLinkRepository
{
    Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void AddPasswordResetToken(PasswordResetToken token);
    void AddAuditLog(UserAuditLog auditLog);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
