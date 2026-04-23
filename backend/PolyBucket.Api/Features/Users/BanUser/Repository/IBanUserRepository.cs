using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Users.BanUser.Repository;

public interface IBanUserRepository
{
    Task<User?> FindByIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
